using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Server.Interface;
using SunSocket.Core;
using SunSocket.Server;

namespace SunSocket.Udp
{
    class Program
    {
        static void Main(string[] args)
        {
            //UdpServer server = new UdpServer(8878, 10, 4 * 1024);
            //server.OnReceived += ReceiveCompleted;
            //server.Start();

            PackageId pId = new PackageId();
            pId.Init();
            ConcurrentDictionary<uint, uint> dict = new ConcurrentDictionary<uint, uint>();
            int i = 5;
            while (i>0)
            {
                Parallel.For(1, 100000, x =>
                {
                    var id = pId.NewId();
                    if (!dict.TryAdd(id, id))
                    {
                        Console.WriteLine(id);
                    }
                });
                Thread.Sleep(1000);
                i--;
            }
            Console.WriteLine("完成了");
            Console.ReadLine();
        }
        static void ReceiveCompleted(object sender, byte[] data)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }
    }
}
