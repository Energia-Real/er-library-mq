using er.library.mq.Domain.Bus;
using er.library.mq.Domain.Commands;
using er.library.mq.Domain.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace er.library.mq.Domain.RabbitMQConfig
{
    public class RabbitMQEventPublisher : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;

        public RabbitMQEventPublisher(ConnectionFactory factory, string exchangeName, string queueName, string routingKey)
        {
            _exchangeName = exchangeName;
            _queueName = queueName;
            _routingKey = routingKey;
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar el exchange y la cola
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);
        }

        
        public Task SendCommand<T>(T command, string actionType) where T : Command
        {
            string routingKey = $"command.{actionType}";
            return PublishMessage(command, routingKey);
        }

        public Task Publish<T>(T @event, string actionType) where T : Event
        {
            string routingKey = GenerateRoutingKey(_routingKey, actionType);
            return PublishMessage(@event, routingKey);
        }

        private Task PublishMessage<T>(T message, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var properties = _channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object> { { Guid.NewGuid().ToString(), "Ready" } };
            _channel.BasicPublish(exchange: _exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
            return Task.CompletedTask;
        }

        private string GenerateRoutingKey(string template, string actionType)
        {
            return template.Replace("*", actionType);
        }
    }
}
