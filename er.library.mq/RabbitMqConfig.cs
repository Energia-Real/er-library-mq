using System.Text.Json;

namespace er.mq.library;

/// <summary>
/// Representa la configuración necesaria para conectarse a un servidor RabbitMQ.
/// Incluye credenciales, el exchange y el virtual host.
/// </summary>
public class RabbitMqConfig
{
    public string HostName { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Carga la configuración desde un archivo JSON externo.
    /// </summary>
    public static RabbitMqConfig Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var configRoot = JsonSerializer.Deserialize<ConfigRoot>(json);

        return configRoot?.RabbitMqConfig ?? throw new InvalidOperationException("RabbitMqConfig section not found in configuration file.");
    }

    /// <summary>
    /// Clase interna que representa el nodo raíz del JSON.
    /// </summary>
    private class ConfigRoot
    {
        public RabbitMqConfig RabbitMqConfig { get; set; } = new RabbitMqConfig();
    }
}
