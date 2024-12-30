let eventSource = null;
let socket = null;
let users = [];
let sessionId = self.crypto.randomUUID().substring(0, 6);
let history = [];

let wsServerAddress = "wss://localhost:44340";
let httpServerAddress = "https://localhost:44340";


const messageInput = document.getElementById("messageInput");
messageInput.addEventListener("keypress", function (event) {
    const messageText = messageInput.value.trim();
    if (event.key === "Enter" && !event.shiftKey) {
        if (messageText.length > 256) {
            return;
        }
        event.preventDefault();
        sendMessage();
    }
});

messageInput.addEventListener("input", function (event) {
    const error = document.getElementById("error-message");
    error.hidden = true;
    sendButton.disabled = false;
    const messageText = messageInput.value.trim();
    if (messageText.length > 256) {
        error.innerText = "Message is too long";
        error.hidden = false;
        const sendButton = document.getElementById("sendButton");
        sendButton.disabled = true;
        return;
    }
});

function connect() {
    setStatus("Connecting...");
    connectWebSocket();
}

let connectionId = null;

function connectWebSocket() {
    const sessionIdEncoded = encodeURIComponent(sessionId);
    socket = new WebSocket(wsServerAddress + "/ws?sessionId=" + sessionIdEncoded);

    socket.onopen = function (event) {
        goToChat();
        messageInput.focus();
    };

    socket.onerror = function (event) {
        setStatus("Could not reach server.");
    };

    socket.onmessage = function (event) {
        const message = JSON.parse(event.data);
        handleMessage(message);
    };

    socket.onclose = function (event) {
        goToIndex();
        console.log("WebSocket is closed.", event);
        let status = event.reason === "" ? "Connection closed." : event.reason;
        setStatus(status);
        clearHistory();
        users = [];
        updateUsersDisplay();
    };
}

function handleMessage(message) {
    console.log("message.Type:" + message.Type);
    switch (message.Type) {
        case "SensorStatusUpdatedMessage":
            console.log("SensorStatusUpdatedMessage");
            console.log(message);
            handleSensorUpdatedMessage(message);
            break;
        case "SensorChangedMessage":
            handleChatMessage(message);
            break;

        case "ChatMessage":
            handleChatMessage(message);
            break;
        case "UserList":
            handleUserList(message);
            break;
        case "History":
            handleHistory(message);
            break;
        case "UserConnected":
            handleUserConnected(message);
            break;
        case "UserDisconnected":
            handleUserDisconnected(message);
            break;
        default:
            console.log("Unknown message type: " + message.Type);
    }
}

function clearHistory() {
    const messages = document.getElementById("messages");
    messages.innerHTML = "";
}

function handleHistory(message) {
    for (let i = 0; i < message.Messages.length; i++) {
        switch (message.Messages[i].Type) {
            case "ChatMessage":
                handleChatMessage(message.Messages[i]);
                break;
            case "UserConnected":
                handleUserConnected(message.Messages[i]);
                break;
            case "UserDisconnected":
                handleUserDisconnected(message.Messages[i]);
                break;
            default:
                console.log("Unknown message type: " + message.Messages[i].Type);
        }
    }
}

function handleUserConnected(message) {
    users.push(message.SessionId);
    updateUsersDisplay();

    addMessage(`*${message.SessionId} connected using ${message.Transport}.`, "status-message");
}

function handleUserDisconnected(message) {
    const index = users.indexOf(message.SessionId);
    if (index > -1) {
        users.splice(index, 1);
    }
    updateUsersDisplay();
    addMessage("* " + message.SessionId + " disconnected.", "status-message");
}

function handleUserList(message) {
    users = message.Users;
    updateUsersDisplay();
}

function updateUsersDisplay() {
    const usersList = document.getElementsByClassName("users");
    for (let i = 0; i < usersList.length; i++) {
        usersList[i].innerHTML = "";
        for (let j = 0; j < users.length; j++) {
            const userElement = document.createElement("div");
            userElement.className = "user";
            userElement.innerText = users[j];
            if (users[j] === sessionId) {
                userElement.classList.add("current-user");
            }
            usersList[i].appendChild(userElement);
        }
    }
}

function handleSensorUpdatedMessage(messageObject) {
    const message = document.createElement("div");

    const messageName = document.createElement("span");
    messageName.innerText = messageObject.SessionId + ": ";
    messageName.className = "message-name";
    message.appendChild(messageName);

    const messageContent = document.createElement("span");
    var o = JSON.parse(messageObject.SensorStateJson);
    //https://localhost:44340/api/ws/sensor-status-updated?sensorId=123&statusJson={"sensorState":"1001", "sensorId":"S002"}
    messageContent.innerText = "sensorState:" + o.sensorState + ", sensorId." + o.sensorId;
    messageContent.className = "message-content";
    message.appendChild(messageContent);

    const messages = document.getElementById("messages");
    messages.appendChild(message);
    messages.scrollTop = messages.scrollHeight;
}

function handleChatMessage(messageObject) {
    const message = document.createElement("div");

    const messageName = document.createElement("span");
    messageName.innerText = messageObject.SessionId + ": ";
    messageName.className = "message-name";
    message.appendChild(messageName);

    const messageContent = document.createElement("span");
    messageContent.innerText = messageObject.Content;
    messageContent.className = "message-content";
    message.appendChild(messageContent);

    const messages = document.getElementById("messages");
    messages.appendChild(message);
    messages.scrollTop = messages.scrollHeight;
}

function addMessage(text, type) {
    const message = document.createElement("div");
    message.innerText = text;
    message.className = type || "message";

    const messages = document.getElementById("messages");
    messages.appendChild(message);
    messages.scrollTop = messages.scrollHeight;
}

function sendMessage() {
    const messageInput = document.getElementById("messageInput");
    const messageText = messageInput.value.trim();
    if (sessionId && messageText) {
        const message = { Type: "ChatMessage", SessionId: sessionId, Content: messageText };
        sendMessageWebSocket(JSON.stringify(message));
        messageInput.value = "";
        messageInput.focus();
    }
}

function sendMessageHttp(message) {
    fetch(httpServerAddress + "/lp/message", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(message),
    })
        .then((response) => {
            if (!response.ok) {
                throw new Error(error);
            }
        })
        .catch((error) => {
            goToIndex();
            setStatus(error);
            console.error("Error:", error);
        });
}

function sendMessageWebSocket(message) {
    socket.send(message);
}

function setStatus(statusText) {
    const status = currentPage().querySelector(".status");
    status.innerText = statusText;
    status.hidden = false;
}

function goToIndex() {
    document.getElementById("homePage").style.display = "block";
    document.getElementById("chatPage").style.display = "none";
}

function goToChat() {
    document.getElementById("homePage").style.display = "none";
    document.getElementById("chatPage").style.display = "block";
}

function currentPage() {
    return document.querySelector('.page[style*="display: block"]');
}
