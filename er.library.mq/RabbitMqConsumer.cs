namespace er.mq.library;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Consumidor de mensajes de RabbitMQ que se conecta a una cola dinámica basada en una clave de enlace (binding key).
/// </summary>
public class RabbitMqConsumer : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly RabbitMqConfig _config;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RabbitMqConsumer"/>.
    /// </summary>
    /// <param name="config">Configuración de RabbitMQ.</param>
    /// <param name="bindingKey">Clave de enlace utilizada para enlazar la cola al exchange.</param>
    public RabbitMqConsumer(RabbitMqConfig config, string bindingKey)
    {
        _config = config;

        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            VirtualHost = _config.VirtualHost,
            UserName = _config.Username,
            Password = _config.Password
        };

        // Establece conexión y canal de manera asincrónica
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Nombre de la cola dinámico basado en la binding key
        _queueName = $"queue-{bindingKey.Replace(".", "_")}";

        // Declara la cola
        _channel.QueueDeclareAsync(
            _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        ).GetAwaiter().GetResult();

        // Vincula la cola al exchange usando la binding key
        _channel.QueueBindAsync(
            _queueName,
            _config.Exchange,
            bindingKey
        ).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Inicia la escucha de mensajes en la cola asociada.
    /// Los mensajes recibidos se muestran en consola.
    /// </summary>
    public void StartListening()
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($" [x] Mensaje recibido en {_queueName}:");
            Console.WriteLine($"     Routing Key: {ea.RoutingKey}");
            Console.WriteLine($"     Mensaje: {message}");
            Console.WriteLine(new string('-', 50));

            await Task.CompletedTask;
        };

        _channel.BasicConsumeAsync(
            _queueName,
            autoAck: true,
            consumer
        ).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Libera de forma asincrónica los recursos utilizados por el consumidor.
    /// Cierra el canal y la conexión.
    /// </summary>
    /// <returns>Tarea completada.</returns>
    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}