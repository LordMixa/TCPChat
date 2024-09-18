using System.Net;
using System.Net.Sockets;
using System.Text;

public class DiscoveryClient
{
    private static readonly int discoveryPort = 5001;
    private static readonly int maxRetries = 10;
    private static readonly int retryDelay = 2000;

    public static void DiscoverServers()
    {
        UdpClient udpClient = new UdpClient(0);
        udpClient.EnableBroadcast = true;

        IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
        string discoveryMessage = "Discover Chat Servers";
        byte[] sendBytes = Encoding.ASCII.GetBytes(discoveryMessage);

        int retryCount = 0;
        bool serverFound = false;

        Thread receiveThread = new Thread(() =>
        {
            while (!serverFound && retryCount < maxRetries)
            {
                IPEndPoint responseEP = new IPEndPoint(IPAddress.Any, discoveryPort);

                try
                {
                    byte[] receiveBytes = udpClient.Receive(ref responseEP);
                    string response = Encoding.ASCII.GetString(receiveBytes);
                    Console.WriteLine($"Received response from {responseEP.Address}: {response}");
                    serverFound = true; 
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }
        });
        receiveThread.Start();

        while (retryCount < maxRetries && !serverFound)
        {
            udpClient.Send(sendBytes, sendBytes.Length, broadcastEP);
            Console.WriteLine($"Discovery request sent. Retry {retryCount + 1}/{maxRetries}");
            retryCount++;

            Thread.Sleep(retryDelay); 
        }

        receiveThread.Join(); 

        if (!serverFound)
        {
            Console.WriteLine("Servers not found. What would you like to do?");
            Console.WriteLine("1. Retry discovery");
            Console.WriteLine("2. Exit");

            string userInput = Console.ReadLine()!;
            if (userInput == "1")
            {
                retryCount = 0; 
                DiscoverServers();
            }
            else
            {
                Console.WriteLine("Exiting discovery...");
                udpClient.Close();
            }
        }
        else
        {
            Console.WriteLine("Discovery completed successfully.");
        }
    }
}
