namespace AiWs.Api.WebSockets;

public static class AiMapWebsockets
{
    public static WebApplication AiUseWebsockets(this WebApplication app)
    {
        app.UseCors("AllowAnyOrigin");
        app.UseWebSockets();
        //        app.MapGet("/", () => @"WebSocket server
        //        test with 
        //  file:///C:/proj/Websockets/AiWs/web-client/index.html
        // API examples:   
        // https://localhost:44340/api/ws/sensor-updated?sensorId=123
        // https://localhost:44340/api/ws/sensor-status-updated?sensorId=123&statusJson={""sensorState"":""1001"", ""sensorId"":""S002""}
        //");
        app.Map("/ws", async (HttpContext context, string sessionId, WebSocketAdapter ws) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await ws.HandleConnect(context, sessionId);
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Expected a WebSocket request");
            }
        });
        return app;
    }
}
