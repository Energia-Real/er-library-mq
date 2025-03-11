namespace er.mq.library;

/// <summary>
/// Representa un mensaje de evento utilizado para la comunicación entre servicios.
/// </summary>
public class EventMessage<T>
{
    /// <summary>
    /// Identificador único del evento.
    /// </summary>
    public string EventId { get; set; }

    /// <summary>
    /// Identificador de correlación para encadenar eventos relacionados en un mismo flujo.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Tipo de evento, por ejemplo: factura.subida.creada.prod.
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Nombre del servicio que emite el evento, por ejemplo: BillsService, AssetService, etc.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Dominio general al que pertenece el evento, como factura, planta, usuario, dispositivo, etc.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// Identificador único de la entidad asociada al evento, como RPU, idPlanta, idUsuario o idDispositivo.
    /// </summary>
    public string EntityId { get; set; }

    /// <summary>
    /// Fecha y hora en que se generó el evento.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Información adicional del evento, como versión o etiquetas personalizadas.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Contenido específico del evento, permitiendo cualquier tipo de datos.
    /// </summary>
    public T Payload { get; set; }
}