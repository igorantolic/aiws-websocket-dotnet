/*
 * 
 * 
  file:///C:/proj/Websockets/AiWs/web-client/index.html
 API examples:   
 https://localhost:44340/api/ws/sensor-updated?sensorId=123
 https://localhost:44340/api/ws/sensor-status-updated?sensorId=123&statusJson={"sensorState":"1001", "sensorId":"S002"}
//********************** INITIALIZATION
 Program.cs

    //+WS
    builder.Services.AddSingleton<MessagingService>();
    builder.Services.AddTransient<WebSocketAdapter>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAnyOrigin", p => p
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Connection-Id"));
    });
    //-WS

    //app.AiUseWebsockets();//+WS

AiMapWebsockets
    AiUseWebsockets - register, cors, map /ws for websockets
    

//other WebSockets/* classes
    WebSockets/Messages.cs - message types records
    WebSockets/MessagingService.cs 
        - IConnectionAdapter - interface with SensorId for subscription of changes of sensor
        - MessagingService --> singeton (Program.cs)
            - TryAddWsSession+TryAddWsSessionInternal / RemoveWsSession
            - SendMessage, 
            - BroadcastMessage, 
            - SetSensorId (subscribe to changes)
            - NotifySensorUpdatedAsync -> called from API to notify clients waiting for notification for particular sensor
    WebSocketAdapter: MessagingService.IConnectionAdapter
            - has WebSocket (to cummunicate with that client) 
            - HandleConnect / CloseConnection
            - Receive
            - SendMessage -> calls handleMessage pointer to ***OnReceiveAsync***
                    handles message "sensorId={id}"

Adding custom message type
    Messages.cs
        [JsonDerivedType(typeof(SensorChangedMessage))]
        public record SensorStatusUpdatedMessage(string SessionId, string SensorStateJson) : Message(nameof(SensorStatusUpdatedMessage));

    on web page:
        function handleMessage(message) {
            console.log("message.Type:" + message.Type);
            switch (message.Type) {
                case "SensorStatusUpdatedMessage":
                    console.log("SensorStatusUpdatedMessage");
                    console.log(message);
                    handleSensorUpdatedMessage(message);
 */ 