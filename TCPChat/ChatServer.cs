using System.Net;
using System.Net.Sockets;
using TCPChat;

public class ChatServer
{
    private static TcpListener? server;
    private static readonly List<TcpClient> clients = new List<TcpClient>();
    private static readonly Dictionary<string, List<string>> privateChats = new Dictionary<string, List<string>>();
    private static readonly Dictionary<TcpClient, User> users = new Dictionary<TcpClient, User>();
    private static readonly string userFilePath = "users.txt";
    private static readonly string historyFilePath = "history.txt";

    public static void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Chat started. Waiting for connection...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        clients.Add(client);
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

        string clientName = "";

        while (string.IsNullOrEmpty(clientName))
        {
            writer.WriteLine("Do you have already account?(Y/N):");
            string check = reader.ReadLine()!;
            if (check == "Y")
            {
                writer.WriteLine("Enter your name:");
                clientName = reader.ReadLine()!;

                if (clientName == null || clientName.Length == 0)
                {
                    writer.WriteLine("Invalid name. Please try again.");
                    clientName = "";
                    continue;
                }

                writer.WriteLine("Enter your password:");
                string pass = reader.ReadLine()!;
                if (IsUserRegistered(clientName, pass))
                {
                    users.Add(client, new User(clientName, pass));
                    writer.WriteLine("Welcome to the chat!");
                }
                else
                {
                    writer.WriteLine("Nickname or password wrong");
                    clientName = "";
                }
            }
            else if (check == "N")
            {
                writer.WriteLine("Enter your name for register:");
                clientName = reader.ReadLine()!;
                if (clientName == null || clientName.Length == 0)
                {
                    writer.WriteLine("Invalid name. Please try again.");
                    clientName = "";
                    continue;
                }
                if (!IsNameExist(clientName))
                {
                    writer.WriteLine("Enter your password:");
                    string pass = reader.ReadLine()!;
                    if (clientName == null || clientName.Length == 0)
                    {
                        writer.WriteLine("Incorrect password");
                        clientName = "";
                    }
                    else
                    {
                        SaveUser(clientName!, pass);
                        users.Add(client, new User(clientName!, pass));
                        writer.WriteLine("Welcome to the chat!");
                    }
                }
                else
                {
                    writer.WriteLine("This nickname already exist");
                    clientName = "";
                }
            }
        }

        Broadcast($"{clientName} joined to chat.", client);

        while (true)
        {
            try
            {
                string? message = reader.ReadLine();
                if (message == null) break;

                ProcessMessage(message, client);
            }
            catch
            {
                break;
            }
        }

        DisconnectClient(client);
    }

    private static bool IsUserRegistered(string userName, string pass)
    {
        var registeredUsers = File.ReadAllLines(userFilePath);
        string[] names = new string[registeredUsers.Length];
        for (int i = 0; i < registeredUsers.Length; i++)
        {
            if (registeredUsers[i].Split(":")[0] == userName && registeredUsers[i].Split(":")[1] == pass)
                return true;
        }
        return false;
    }
    private static bool IsNameExist(string userName)
    {
        var registeredUsers = File.ReadAllLines(userFilePath);
        string[] names = new string[registeredUsers.Length];
        for (int i = 0; i < registeredUsers.Length; i++)
        {
            names[i] = registeredUsers[i].Split(":")[0];
        }
        return names.Contains(userName);
    }
    private static void SaveUser(string userName, string pass)
    {
        using (StreamWriter fileWriter = new StreamWriter(userFilePath, true))
        {
            fileWriter.WriteLine(userName + ":" + pass);
        }
    }

    private static void ProcessMessage(string message, TcpClient sender)
    {
        string senderName = users[sender].UserName;
        if (message.StartsWith("/private"))
        {
            string[] parts = message.Split(' ');
            if (parts.Length >= 3)
            {
                string targetUser = parts[1];
                string privateMessage = string.Join(" ", parts.Skip(2));
                SendPrivateMessage(senderName, targetUser, privateMessage);
            }
        }
        else
        {
            Broadcast($"{senderName} {DateTime.Now}: {message}", sender);
        }
    }

    private static void SendPrivateMessage(string senderName, string targetUser, string message)
    {
        var recipient = users.FirstOrDefault(c => c.Value.UserName == targetUser).Key;
        if (recipient != null)
        {
            SendMessage(recipient, $"Private message from {senderName}: {message}");
            SavePrivateMessage(senderName, targetUser, message);
        }
    }

    private static void SavePrivateMessage(string sender, string receiver, string message)
    {
        string chatId = GetPrivateChatId(sender, receiver);
        if (!privateChats.ContainsKey(chatId))
        {
            privateChats[chatId] = new List<string>();
        }
        privateChats[chatId].Add($"{sender}: {message}");
    }

    private static string GetPrivateChatId(string user1, string user2)
    {
        return user1.CompareTo(user2) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
    }

    private static void Broadcast(string message, TcpClient sender)
    {
        SaveHistory(message);
        foreach (var client in clients)
        {
            if (client != sender)
            {
                SendMessage(client, message);
            }
        }
        Console.WriteLine(message);
    }

    private static void SendMessage(TcpClient client, string message)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
            writer.WriteLine(message); 
        }
        catch
        {
            DisconnectClient(client);
        }
    }

    private static void DisconnectClient(TcpClient client)
    {
        if (users.TryGetValue(client, out User? user))
        {
            clients.Remove(client);
            users.Remove(client);
            client.Close();
            Broadcast($"{user.UserName} left chat.", null!);
        }
    }
    public static void SaveHistory(string message)
    {
        using (StreamWriter fileWriter = new StreamWriter(historyFilePath, true))
        {
            fileWriter.WriteLine(message);
        }
    }
}
