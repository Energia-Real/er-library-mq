namespace er.mq.library;

using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Publicador de mensajes en RabbitMQ. Permite enviar mensajes a un exchange específico utilizando una clave de enrutamiento (routing key).
/// </summary>
public class RabbitMqPublisher : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly RabbitMqConfig _config;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RabbitMqPublisher"/> con la configuración especificada.
    /// </summary>
    /// <param name="config">Configuración de conexión y exchange de RabbitMQ.</param>
    public RabbitMqPublisher(RabbitMqConfig config)
    {
        _config = config;

        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            VirtualHost = _config.VirtualHost,
            UserName = _config.Username,
            Password = _config.Password
        };

        // Establece la conexión y el canal
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Valida que el exchange exista (opcional, pero recomendable para detección temprana de errores)
        _channel.ExchangeDeclarePassiveAsync(_config.Exchange).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Publica un mensaje en el exchange configurado usando la routing key especificada.
    /// </summary>
    /// <param name="routingKey">Clave de enrutamiento que determina la cola destino.</param>
    /// <param name="message">Contenido del mensaje a enviar.</param>
    /// <returns>Tarea que representa la operación asincrónica.</returns>
    public async Task PublishAsync(string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties
        {
            ContentType = "text/plain"
        };

        await _channel.BasicPublishAsync(
            _config.Exchange,
            routingKey,
            mandatory: false,
            properties,
            body
        );

        Console.WriteLine($" [x] Sent '{routingKey}':'{message}'");
    }

    /// <summary>
    /// Libera los recursos asincrónicamente, cerrando el canal y la conexión.
    /// </summary>
    /// <returns>Tarea completada.</returns>
    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}