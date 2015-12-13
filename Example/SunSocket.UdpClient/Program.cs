using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SunSocket.Server.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Server;

namespace SunSocket.UdpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer server = new UdpServer(8879, 10, 4 * 1024);
            server.OnReceived += ReceiveCompleted;
            server.Start();
            Console.ReadLine();
            while (true)
            {
                server.SendAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8878), new SendData() {Data = Encoding.UTF8.GetBytes("我爱我的祖国啊啊啊啊,测试测试") });
                Console.ReadLine();
            }
        }
        static void ReceiveCompleted(object sender, byte[] data)
        {
            IUdpSession session = sender as IUdpSession;
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }
    }
}
