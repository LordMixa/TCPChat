using System.Net;
using System.Net.Sockets;
using System.Text;

public class DiscoveryServer
{
    private static readonly int discoveryPort = 5001; 

    public static void StartServer()
    {
        UdpClient udpListener = new UdpClient(discoveryPort);
        udpListener.EnableBroadcast = true; 
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, discoveryPort);

        Console.WriteLine("Discovery server is running. Waiting for discovery requests...");

        while (true)
        {
            try
            {
                byte[] receiveBytes = udpListener.Receive(ref groupEP);
                string receivedData = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine($"Received discovery request from {groupEP.Address}: {receivedData}");

                string response = $"Chat server is here at {groupEP.Address}:{discoveryPort}";
                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                udpListener.Send(responseBytes, responseBytes.Length, groupEP);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
    }
}
