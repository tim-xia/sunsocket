using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SunSocket.Core.Interface;
using SunRpc.Client;
using System.Diagnostics;

namespace Rpc.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var loger = new Loger();
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            var client = new RpcClient(endPoint, loger);
            client.Connect();
            Console.WriteLine("连接成功");
            Console.ReadLine();
            int count = 10000;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                var t = client.Invoke<int>("Caculator", "Add", 5, 5);
                var data = t.Result;
            }
            sw.Stop();
            Console.WriteLine("RPC完成{0}次调用，运行时间：{1} 秒{2}毫秒",count, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
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
            Console.WriteLine(message);
        }

        public void Fatal(Exception e)
        {
            Console.WriteLine(e.Message);
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
