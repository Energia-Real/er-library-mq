using RabbitMQ.Client;

namespace er.mq.library;

/// <summary>
/// Configuración inicial de RabbitMQ, incluyendo la declaración de exchanges y colas necesarias,
/// así como la configuración de un exchange alternativo para mensajes no enrutados (Dead Letter Exchange - DLX).
/// </summary>
public class RabbitMqSetup : IAsyncDisposable
{
    private readonly RabbitMqConfig _config;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RabbitMqSetup"/> con la configuración de RabbitMQ.
    /// </summary>
    /// <param name="config">Configuración de conexión a RabbitMQ.</param>
    public RabbitMqSetup(RabbitMqConfig config)
    {
        _config = config;

        var factory = new ConnectionFactory
        {
            HostName = config.HostName,
            VirtualHost = config.VirtualHost,
            UserName = config.Username,
            Password = config.Password
        };

        // Crea la conexión y el canal asincrónicamente (aunque se espera sincrónicamente aquí).
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Configura el entorno de RabbitMQ, incluyendo:
    /// - Un exchange alternativo para mensajes no enrutados.
    /// - Una cola para capturar esos mensajes.
    /// - El exchange principal, enlazado al exchange alternativo.
    /// </summary>
    public async Task SetupAsync()
    {
        // Nombre del exchange alternativo (DLX)
        const string alternateExchange = "unrouted-messages-exchange";

        // 1️⃣ Declara el exchange alternativo (fanout)
        await _channel.ExchangeDeclareAsync(
            exchange: alternateExchange,
            type: "fanout",
            durable: true
        );

        // 2️⃣ Declara la cola para mensajes no enrutados
        await _channel.QueueDeclareAsync(
            queue: "unrouted-messages-queue",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // 3️⃣ Vincula la cola al exchange alternativo
        await _channel.QueueBindAsync(
            queue: "unrouted-messages-queue",
            exchange: alternateExchange,
            routingKey: string.Empty
        );

        // 4️⃣ Declara el exchange principal (con soporte para alternate-exchange)
        var args = new Dictionary<string, object>
        {
            { "alternate-exchange", alternateExchange }
        };

        await _channel.ExchangeDeclareAsync(
            exchange: _config.Exchange,
            type: "topic",
            durable: true,
            arguments: args
        );

        Console.WriteLine($"Exchange '{_config.Exchange}' con DLQ configurado correctamente.");
    }

    /// <summary>
    /// Libera de forma asincrónica los recursos utilizados por la configuración.
    /// Cierra el canal y la conexión.
    /// </summary>
    /// <returns>Una tarea completada.</returns>
    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}