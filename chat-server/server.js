// server.js
const express = require("express");
const http = require("http");
const { Server } = require("socket.io");

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: "*", // allow any client
  },
});

// Store usernames: { socket.id: username }
const users = {};

io.on("connection", (socket) => {
  console.log("A user connected:", socket.id);

  // Ask the client for their name
  socket.emit("request_name");

  // Receive username
  socket.on("set_name", (name) => {
    users[socket.id] = name;
    console.log(`${name} joined the chat`);

    // Confirm to client that username is registered
    socket.emit("name_confirmed", { name });

    // Broadcast join message
    io.emit("chat_message", { user: "Server", message: `${name} has joined the chat!` });
  });

  // Receive chat messages
  socket.on("chat_message", (data) => {
    let msg = "";

    if (typeof data === "string") msg = data;
    else if (data.message) msg = data.message;

    const username = users[socket.id] || "Unknown";
    console.log(`${username}: ${msg}`);

    io.emit("chat_message", { user: username, message: msg });
  });

  // Handle disconnect
  socket.on("disconnect", () => {
    const username = users[socket.id];
    console.log("User disconnected:", username || socket.id);
    if (username) {
      io.emit("chat_message", { user: "Server", message: `${username} has left the chat.` });
      delete users[socket.id];
    }
  });
});

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
  console.log(`✅ Server running on port ${PORT}`);
});
