using System.Text.Json;
using Pcf.MessageBus.Abstractions;

namespace Pcf.MessageBus.RabbitMq.Serialization;

public sealed class SystemTextJsonEventSerializer: IEventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public byte[] Serialize<TEvent>(TEvent @event) where TEvent : IntegrationEvent  => JsonSerializer.SerializeToUtf8Bytes(@event, JsonOptions);
    

    public TEvent Deserialize<TEvent>(byte[] body) where TEvent : IntegrationEvent => JsonSerializer.Deserialize<TEvent>(body, JsonOptions)
        ?? throw new InvalidOperationException($"Cannot deserialize {typeof(TEvent).FullName}");
} 