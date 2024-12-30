using System.Text.Json.Serialization;

namespace AiWs.Api.WebSockets;

// Required for polymorphic deserialization
[JsonDerivedType(typeof(SensorChangedMessage))]
[JsonDerivedType(typeof(SensorStatusUpdatedMessage))]
[JsonDerivedType(typeof(WsMessage))]
[JsonDerivedType(typeof(SessionList))]
[JsonDerivedType(typeof(SessionConnected))]
[JsonDerivedType(typeof(SessionDisconnected))]
[JsonDerivedType(typeof(History))]
public record Message(string Type);

public record SensorStatusUpdatedMessage(string SessionId, string SensorStateJson) : Message(nameof(SensorStatusUpdatedMessage));
public record SensorChangedMessage(string SessionId, string Content) : Message(nameof(SensorChangedMessage));
public record WsMessage(string SessionId, string Content) : Message(nameof(WsMessage));

public record SessionList(IEnumerable<string> Users) : Message(nameof(SessionList));

public record SessionConnected(string SessionId, string Transport) : Message(nameof(SessionConnected));

public record SessionDisconnected(string SessionId) : Message(nameof(SessionDisconnected));

public record History(IEnumerable<Message> Messages) : Message(nameof(History));