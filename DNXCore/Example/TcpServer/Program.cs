using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SunSocket.Server;
using SunSocket.Server.Config;
using SunSocket.Server.Session;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;

namespace TcpServerOne
{
    public class MyServer : TcpServer
    {
        public MyServer(TcpServerConfig config, ILoger loger) : base(config, loger)
        { }
        public override void OnReceived(ITcpSession session, IDynamicBuffer dataBuffer)
        {
            var result = new byte[dataBuffer.DataSize];
            Buffer.BlockCopy(dataBuffer.Buffer, 0, result, 0, dataBuffer.DataSize);
            session.SendAsync(new SendData { Buffer = result });
        }
    }
    class Program
    {
        static List<string> li = new List<string>();
        static Loger loger = new Loger();
        static void Main(string[] args)
        {
            TcpServerConfig configOne = new TcpServerConfig { Name = "one", IP = "127.0.0.1", Port = 8088, BufferSize = 1024, MaxFixedBufferPoolSize = 1024 * 4, MaxConnections = 8000 };
            MyServer listener = new MyServer(configOne, loger);
            listener.Start();
            MonitorConfig monitorConfig = new MonitorConfig();
            monitorConfig.WorkDelayMilliseconds = 10000;
            monitorConfig.TimeoutMilliseconds = 10000;
            TcpMonitor monitor = new TcpMonitor(monitorConfig);
            monitor.AddServer(listener);
            monitor.Start();
            Console.WriteLine("服务器已启动");
            Console.ReadLine();
        }
        //static byte[] data = Encoding.UTF8.GetBytes("测试数据服务器返回");
        //static SendData sdata = new SendData() { Buffer = data, Offset = 0 };
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
