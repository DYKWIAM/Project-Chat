//using System;
//using System.Threading.Tasks;
//using System.Collections.Generic;
using SocketIOClient;

class Program
{
    // Map usernames to console colors
    static Dictionary<string, ConsoleColor> userColors = new Dictionary<string, ConsoleColor>();
    static ConsoleColor[] availableColors = new ConsoleColor[]
    {
        ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Magenta,
        ConsoleColor.Yellow, ConsoleColor.Blue, ConsoleColor.DarkCyan,
        ConsoleColor.DarkYellow, ConsoleColor.DarkMagenta
    };
    static int colorIndex = 0;

    static async Task Main(string[] args)
    {
        var socket = new SocketIOClient.SocketIO("https://project-chat-bepj.onrender.com");

        // Connected event
        socket.OnConnected += (sender, e) =>
        {
            Console.WriteLine("✅ Connected to server!");
        };

        // Handle server requesting name
        socket.On("request_name", async (response) =>
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine() ?? "Unknown";
            await socket.EmitAsync("set_name", name);
        });

        // Wait until the server confirms the username
        var nameConfirmed = new TaskCompletionSource<bool>();
        socket.On("name_confirmed", (response) =>
        {
            nameConfirmed.SetResult(true);
        });

        // Handle incoming chat messages
        socket.On("chat_message", (response) =>
        {
            try
            {
                var data = response.GetValue<ChatMessage>();
                PrintMessage(data.user ?? "Unknown", data.message ?? "");
            }
            catch
            {
                Console.WriteLine("Received unknown message: " + response.ToString());
            }
        });

        // Connect to the server
        await socket.ConnectAsync();

        // Wait until username is confirmed before sending messages
        await nameConfirmed.Task;

        // Input loop
        while (true)
        {
            string? input = Console.ReadLine();
            if (input?.ToLower() == "exit") break;

            await socket.EmitAsync("chat_message", new Dictionary<string, string>
            {
                { "message", input }
            });
        }

        await socket.DisconnectAsync();
    }

    // Print message with color per user
    static void PrintMessage(string user, string message)
    {
        if (!userColors.ContainsKey(user))
        {
            // Assign a new color
            userColors[user] = availableColors[colorIndex % availableColors.Length];
            colorIndex++;
        }

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = userColors[user];
        Console.Write($"{user}: ");
        Console.ForegroundColor = originalColor;
        Console.WriteLine(message);
    }

    // Class to deserialize incoming chat messages
    public class ChatMessage
    {
        public string? user { get; set; }
        public string? message { get; set; }
    }
}
