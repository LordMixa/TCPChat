using System.Net;
using System.Net.Sockets;
using System.Text;

public class DiscoveryClient
{
    private static readonly int discoveryPort = 5001;

    public static void DiscoverServers()
    {
        UdpClient udpClient = new UdpClient(discoveryPort);
        udpClient.EnableBroadcast = true;  

        IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
        string discoveryMessage = "Discover Chat Servers";
        byte[] sendBytes = Encoding.ASCII.GetBytes(discoveryMessage);

        udpClient.Send(sendBytes, sendBytes.Length, broadcastEP);
        Console.WriteLine("Discovery request sent.");
        
        Thread receiveThread = new Thread(() =>
        {
            UdpClient receiveClient = new UdpClient(discoveryPort);
            receiveClient.EnableBroadcast = true; 
            IPEndPoint responseEP = new IPEndPoint(IPAddress.Any, discoveryPort);

            while (true)
            {
                try
                {
                    byte[] receiveBytes = receiveClient.Receive(ref responseEP);
                    string response = Encoding.ASCII.GetString(receiveBytes);
                    Console.WriteLine($"Received response from {responseEP.Address}: {response}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }
        });

        receiveThread.Start();
    }
}
