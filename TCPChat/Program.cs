namespace TCPChat
{
    public class Program
    {
        static void Main(string[] args)
        {
            Thread serverThread = new Thread(DiscoveryServer.StartServer);
            serverThread.Start();
            //ChatServer.StartServer();
        }
    }
}
