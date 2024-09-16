using System.Net.Sockets;

class ChatClient
{
    private TcpClient? client;
    private StreamReader? reader;
    private StreamWriter? writer;
    private string? name;

    public void ConnectToServer(string serverAddress)
    {
        client = new TcpClient(serverAddress, 5000);
        NetworkStream stream = client.GetStream();

        reader = new StreamReader(stream);
        writer = new StreamWriter(stream) { AutoFlush = true };

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        while (true)
        {
            string message = Console.ReadLine()!;
            SendMessage(message);
        }
    }

    private void SendMessage(string message)
    {
        writer!.WriteLine(message);
    }

    private void ReceiveMessages()
    {
        while (true)
        {
            try
            {
                string? message = reader!.ReadLine();
                if (message == null)
                {
                    Console.WriteLine("Disconnect from server.");
                    break;
                }
                Console.WriteLine(message);
            }
            catch
            {
                Console.WriteLine("Disconnect from server.");
                break;
            }
        }
    }
}
