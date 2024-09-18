using System.Text;

namespace TCPClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DiscoveryClient.DiscoverServers();
            //ChatClient client = new ChatClient();
            //client.ConnectToServer("127.0.0.1");
        }
    }
}
