using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Server;

namespace SunSocket.Udp
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpAsyncServer server = new UdpAsyncServer(8878, 10, 4 * 1024);
            server.OnReceived += ReceiveCompleted;
            server.Start();
            Console.ReadLine();
        }
        static void ReceiveCompleted(object sender, byte[] data)
        {
            IUdpSession session = sender as IUdpSession;
            Console.WriteLine(Encoding.UTF8.GetString(data));
            session.SendAsync(new SendData() {Buffer=data});
        }
    }
}
