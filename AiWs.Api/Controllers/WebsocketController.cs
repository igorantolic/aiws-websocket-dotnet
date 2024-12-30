using AiWs.Api.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Text;

namespace AiWs.Api.Controllers;

/*
 file:///C:/proj/Websockets/AiWs/web-client/index.html
 API examples:   
 https://localhost:44340/api/ws/sensor-updated?sensorId=123
 https://localhost:44340/api/ws/sensor-status-updated?sensorId=123&statusJson={"sensorState":"1001", "sensorId":"S002"}
*/

[Route("api/ws")]
[ApiController]
public class WebsocketController(MessagingService messagingService, ILogger<WebSocketAdapter> logger) : Controller
{
    bool validateAuthToken()
    {
        Request.Headers.TryGetValue("Authorization", out StringValues headerValues);
        string? jsonWebToken = headerValues.FirstOrDefault();
        if (
            string.IsNullOrEmpty(jsonWebToken)
            || jsonWebToken != "Bearer Y6t2HJX4jFQsCY6vl0CxxCwgmkcFpxIrzndmBjwNb9oPUH3qu5CNRR4MWqSvLy9ew0E1mnYo8LCmlZ2ksboKNVb9JjlMFoyVI9mUuoZAB4ZY2AFAtdg7hl0jjQJkIQEEYSw9xQSQItWMgCvZ2AEmvY"
            )
        {
            this.HttpContext.Response.StatusCode = 401;
            var binary = Encoding.UTF8.GetBytes("Not authorized");
            this.HttpContext.Response.BodyWriter.Write(binary);
            return false;
        }
        return true;
    }

    [Route("connections")]
    [HttpGet]
    public IEnumerable<ConnectionInfo> GetConnectins()
    {
        if (!validateAuthToken()) return null;

        try
        {
            var connections = messagingService.GetConnectins();
            var items = new List<ConnectionInfo>();
            foreach (var c in connections.Keys)
            {
                items.Add(new ConnectionInfo() { sessionId = c, sensorId = connections[c].SensorId });
            }
            return items;
        }
        catch (Exception ex)
        {
            //AiOtel.Error(ex);
            throw;
        }
    }

    [Route("sensor-updated")]
    [HttpGet]
    public async Task<List<string>> SensorUpdatedAsync(string sensorId)
    {
        if (!validateAuthToken()) return null;
        try
        {
            return await messagingService.NotifySensorUpdatedAsync(sensorId);
        }
        catch (Exception ex)
        {
            //AiOtel.Error(ex);
            throw;
        }
    }

    [Route("sensor-status-updated")]
    [HttpGet]
    public async Task<List<string>> SensorStatusUpdatedAsync(string sensorId, string statusJson)
    {
        if (!validateAuthToken()) return null;
        try
        {
            return await messagingService.NotifySensorStatusUpdatedAsync(sensorId, statusJson);
        }
        catch (Exception ex)
        {
            //AiOtel.Error(ex);
            throw;
        }
    }


    public class ConnectionInfo
    {
        public string sessionId { get; set; }
        public string sensorId { get; set; }
    }
}
