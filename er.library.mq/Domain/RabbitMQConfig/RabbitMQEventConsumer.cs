using er.library.mq.Domain.Bus;
using er.library.mq.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace er.library.mq.Domain.RabbitMQConfig
{
    public class RabbitMQEventConsumer : IEventHandler
    {
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _exchangeName;
        private readonly string _routingKey;

        public RabbitMQEventConsumer(ConnectionFactory factory, IServiceProvider serviceProvider, string queueName, string exchangeName, string routingKey)
        {
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _exchangeName = exchangeName;
            _routingKey = routingKey;

            _queueName = queueName;
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _queueName, exchange: exchangeName, routingKey: _routingKey);

            _serviceProvider = serviceProvider;
        }

        public void StartListening<T>() where T : Event
        {
            var consumer = new EventingBasicConsumer(_channel);
            int retries = 0;
            consumer.Received += async (model, ea) =>
            {
                try
                {


                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(messageJson);
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetService<IEventHandler<T>>();

                    if (handler != null)
                    {

                        //Add ConsumerTag to dictionary after process message (the dictionary of header initializing with 1 register)
                        if (
                            !ea.BasicProperties.Headers.ContainsKey(ea.ConsumerTag))
                        {
                            await handler.Handle(message);
                            ea.BasicProperties.Headers.Add(ea.ConsumerTag, "ack");
                        }

                        _channel.BasicAck(ea.DeliveryTag, false);
                    }

                    //Publish message every microservices
                    if (
                        //Return all ConsumerTags added 
                        !(ea.BasicProperties.Headers.Count >=
                        //Return the count of consumer 
                        _channel.ConsumerCount(_queueName) + 1)
                        )
                    {
                        _channel.BasicPublish(exchange: _exchangeName, routingKey: ea.RoutingKey, body: ea.Body, basicProperties: ea.BasicProperties);
                    }
                }
                catch (Exception ex)
                {
                    retries++;
                    _channel.BasicAck(ea.DeliveryTag, false);
                    //Re publish if exception
                    if (retries <= 3)
                    {
                        _channel.BasicPublish(exchange: _exchangeName, routingKey: ea.RoutingKey, body: ea.Body, basicProperties: ea.BasicProperties);
                    }
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }
    }

}
