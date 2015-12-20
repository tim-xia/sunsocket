using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using SunSocket.Server.Config;
using SunSocket.Server.Session;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;
using SunSocket.Server.Protocol;
using System.Diagnostics;

namespace RUdpClient
{
    public class TestServer : SunSocket.Server.RUdpServer
    {
        public TestServer(RUdpServerConfig config, ILoger loger) : base(config, loger)
        { }
        static byte[] data = Encoding.UTF8.GetBytes("测试数据服务器返回");
        static int count = 0;
        public override void OnReceived(IRUdpSession session, IDynamicBuffer dataBuffer)
        {
            var result = new byte[dataBuffer.DataSize];
            Buffer.BlockCopy(dataBuffer.Buffer, 0, result, 0, dataBuffer.DataSize);
            var txt = Encoding.UTF8.GetString(result);
            Console.WriteLine(txt);
            session.SendAsync(data);
        }
    }
    class Program
    {
        static Loger loger = new Loger();
        static void Main(string[] args)
        {
            RUdpServerConfig configOne = new RUdpServerConfig { ServerId = 1, Name = "one", IP = "127.0.0.1", Port = 8089, BufferSize = 1024, MaxFixedBufferPoolSize = 1024 * 4, MaxConnections = 8000 };
            TestServer listener = new TestServer(configOne, loger);
            listener.Start();
            var session=listener.SessionPool.Pop();
            session.EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            Console.WriteLine("服务器已启动");
            Console.ReadLine();
            int i = 0,max= 10000;
            Stopwatch sw = new Stopwatch();
            sw.Start();//开始记录时间
            while (i<=max)
            {
                session.SendAsync(Encoding.UTF8.GetBytes("测试数据服务器返回"+i));
                i++;
            }
            sw.Stop();
            Console.WriteLine("{0}次同步查询完成，运行时间：{1} 秒{2}毫秒", i, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
            Console.ReadLine();
        }
    }
    public class Loger : ILoger
    {
        public void Debug(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Debug(string message)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception e)
        {
            throw new NotImplementedException();
        }

        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void Fatal(Exception e)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message)
        {
            throw new NotImplementedException();
        }

        public void Info(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void Log(string message)
        {
            throw new NotImplementedException();
        }

        public void Trace(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception e)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message)
        {
            throw new NotImplementedException();
        }
    }
}
