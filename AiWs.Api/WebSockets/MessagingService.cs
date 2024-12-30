using System.Collections.Concurrent;

namespace AiWs.Api.WebSockets;


public interface IConnectionAdapter
{
    public string SensorId { get; set; }
    Task CloseConnection(string reason);
    Task SendMessage(Message message);
}

public class MessagingService(ILogger<MessagingService> logger)
{
    private readonly ConcurrentDictionary<string, IConnectionAdapter> Connections = new();
    private readonly ConcurrentQueue<Message> History = new();

    public async Task<string?> TryAddWsSession(IConnectionAdapter connection, string sessionId)
    {
        if (Connections.ContainsKey(sessionId))
        {
            return $"sessionId '{sessionId}' already taken";
        }

        Connections.TryAdd(sessionId, connection);
        //var everyoneElse = Connections.Where(x => x.Key != sessionId).Select(x => x.Value);
        //await BroadcastMessage(sessionConnectedMessage, everyoneElse);
        //await SendMessage(connection, new History(History.TakeLast(100)));
        //await SendMessage(connection, new SessionList(Connections.Keys));
        return null;
    }


    public async Task RemoveWsSession(string sessionId)
    {
        Connections.TryRemove(sessionId, out _);
        var msg = new SessionDisconnected(sessionId);
        // await BroadcastMessage(msg);
    }

    public Task SendMessage(IConnectionAdapter connection, Message message)
    {
        logger.LogInformation("Sending message: {message}", message);

        return connection.SendMessage(message);
    }

    //public async Task BroadcastMessage(Message message, IEnumerable<IConnectionAdapter>? receivers = null)
    //{
    //    logger.LogInformation("Broadcasting message: {message}", message);

    //    History.Enqueue(message);

    //    foreach (var connection in receivers ?? Connections.Values)
    //    {
    //        await connection.SendMessage(message);
    //    }
    //}

    //set last sensor
    internal void SetSubscriptionToSensorId(String sessionId, String SensorId)
    {
        if (Connections.ContainsKey(sessionId))
        {
            var connection = Connections[sessionId];
            connection.SensorId = SensorId;
        }
    }

    //for API
    internal ConcurrentDictionary<string, IConnectionAdapter> GetConnectins()
    {
        return Connections;
    }

    private List<KeyValuePair<string, IConnectionAdapter>>? GetConnectionsBySensorId(string sensorId)
    {
        var connections = Connections.Where(c => c.Value.SensorId == sensorId).ToList();
        if (connections == null || connections.Count == 0)
            return null;
        return connections;
    }

    #region notify invoked by API
    internal async Task<List<string>> NotifySensorUpdatedAsync(string sensorId)
    {
        var updatedConnections = new List<string>();
        var connections = GetConnectionsBySensorId(sensorId);
        if (connections == null) return updatedConnections;

        foreach (var connection in connections)
        {
            var msg = new SensorChangedMessage(connection.Key, $"sensor-upaded:{sensorId}");
            await connection.Value.SendMessage(msg);
            updatedConnections.Add(connection.Key);
        }
        return updatedConnections;
    }



    internal async Task<List<string>> NotifySensorStatusUpdatedAsync(string sensorId, string statusJson)
    {
        var connections = GetConnectionsBySensorId(sensorId);
        var updatedConnections = new List<string>();
        if (connections == null) return updatedConnections;

        foreach (var connection in connections)
        {
            var msg = new SensorStatusUpdatedMessage(connection.Key, statusJson);
            await connection.Value.SendMessage(msg);
            updatedConnections.Add(connection.Key);
        }
        return updatedConnections;
    }
    #endregion
}