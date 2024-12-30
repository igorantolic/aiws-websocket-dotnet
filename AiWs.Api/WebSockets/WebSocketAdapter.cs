using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace AiWs.Api.WebSockets;

public class WebSocketAdapter(MessagingService messagingService, ILogger<WebSocketAdapter> logger) : IConnectionAdapter
{
    private WebSocket Socket;

    public string SensorId { get; set; } = "";

    public async Task SendMessage(Message message)
    {
        var messageString = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageString));
        await Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task HandleConnect(HttpContext context, string sessionId)
    {
        Socket = await context.WebSockets.AcceptWebSocketAsync();

        var addUserError = await messagingService.TryAddWsSession(this, sessionId);
        if (addUserError != null)
        {
            await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, addUserError, default);
            return;
        }

        try
        {
            await Receive(Socket, (string messageString) => OnReceiveAsync(messageString ?? "", sessionId));

            await messagingService.RemoveWsSession(sessionId);
        }
        catch (WebSocketException e)
        {
            await messagingService.RemoveWsSession(sessionId);
            logger.LogError(e, "WebSocket error");
        }
    }

    public async Task CloseConnection(string reason)
    {
        await Socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, reason, default);
    }

    private async Task Receive(WebSocket socket, Func<string, Task> handleMessage)
    {
        var buffer = new byte[1024 * 2];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
            if (result.EndOfMessage == false)
            {
                await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Max message size is 2KiB.", default);
                return;
            }
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await Socket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, default);
                return;
            }
            if (result.MessageType != WebSocketMessageType.Text)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Expected text message", default);
                return;
            }
            var messageString = Encoding.UTF8.GetString(buffer[..result.Count]);

            await handleMessage(messageString);
        }
    }

    public async Task OnReceiveAsync(string messageString, string sessionId)
    {
        var message = JsonSerializer.Deserialize<WsMessage>(messageString);
        if (message == null)
        {
            var error = $"Invalid message '{messageString}'";
            await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, error, default);
            await messagingService.RemoveWsSession(sessionId);
            return;
        }

        if (message.Content.StartsWith("sensorId="))
        {
            try
            {
                var sensorId = message.Content.Substring(9);
                messagingService.SetSubscriptionToSensorId(sessionId, sensorId);
                //notify sucessfull subscribe
                var msg = new SensorChangedMessage(sessionId, $"sensor-upate-subscribed:{sensorId}");
                await SendMessage(msg);
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //await messagingService.BroadcastMessage(message);
    }

}