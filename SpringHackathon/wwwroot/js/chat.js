"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;
var messagesContainer = document.querySelector(".messages_container");
connection.on("ReceiveMessage", function (username, message) {
    var isSentByMe = username === currentUser.userName;

    var divBubble = document.createElement("div");
    divBubble.className = isSentByMe ? "bubble me" : "bubble you";
    var divDP = document.createElement("div");
    divDP.className = "dp";

    // Create the SVG element
    var svgElement = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svgElement.setAttribute("viewBox", "0 0 448 512");
    svgElement.setAttribute("width", "100");
    svgElement.setAttribute("title", "user");


    var currentTime = new Date().toLocaleTimeString();

    divBubble.style.setProperty('--time', '"' + currentTime + '"');
    console.log(currentTime);


    // Create the path element within SVG
    var pathElement = document.createElementNS("http://www.w3.org/2000/svg", "path");
    pathElement.setAttribute("d", "M224 256c70.7 0 128-57.3 128-128S294.7 0 224 0 96 57.3 96 128s57.3 128 128 128zm89.6 32h-16.7c-22.2 10.2-46.9 16-72.9 16s-50.6-5.8-72.9-16h-16.7C60.2 288 0 348.2 0 422.4V464c0 26.5 21.5 48 48 48h352c26.5 0 48-21.5 48-48v-41.6c0-74.2-60.2-134.4-134.4-134.4z");

    // Append path element to the SVG
    svgElement.appendChild(pathElement);

    // Append SVG to the 'dp' div
    divDP.appendChild(svgElement);

    // Append 'dp' div to the message bubble
    divBubble.appendChild(divDP);
    var pararaph = document.createElement("p");

    var div1 = document.createElement("div");
    div1.appendChild(pararaph);
    divBubble.appendChild(div1);
    messagesContainer.appendChild(divBubble);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    pararaph.textContent = `${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = currentUser;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user.userName, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});