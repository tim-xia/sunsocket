using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using SunSocket.Server;
using SunSocket.Server.Config;
using SunSocket.Server.Session;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;
using SunSocket.Server.Protocol;

namespace SunSocket
{
    class Program
    {
        static List<string> li = new List<string>();
        static TcpServerConfig config = new TcpServerConfig { BufferSize = 1024, MaxConnections = 8000 };
        static Loger loger = new Loger();
        static void Main(string[] args)
        {
            Server.TcpListener listener = new Server.TcpListener(config,new ServerEndPoint() {Name="one",IP="127.0.0.1",Port=8088}, loger, GetProtocol);
            listener.AsyncServer.OnReceived += ReceiveCommond;
            listener.Start();
            Server.TcpListener listenerOne = new Server.TcpListener(config, new ServerEndPoint() { Name = "two", IP = "127.0.0.1", Port = 9988 }, loger, GetProtocol);
            listenerOne.AsyncServer.OnReceived += ReceiveCommond;
            listenerOne.Start();
            MonitorConfig monitorConfig = new MonitorConfig();
            monitorConfig.WorkDelayMilliseconds = 10000;
            monitorConfig.TimeoutMilliseconds = 10000;
            TcpMonitor monitor = new TcpMonitor(monitorConfig);
            monitor.AddServer(listener.AsyncServer);
            monitor.AddServer(listenerOne.AsyncServer);
            monitor.Start();
            Console.WriteLine("服务器已启动");
            Console.ReadLine();
        }
        public static ITcpPacketProtocol GetProtocol()
        {
            return new TcpPacketProtocol(config.BufferSize, config.MaxBufferPoolSize, loger);
        }
        static byte[] data = Encoding.UTF8.GetBytes("测试数据服务器返回");
        static SendData sdata = new SendData() { Buffer = data, Offset = 0};
        public static void ReceiveCommond(object sender, IDynamicBuffer data)
        {
            TcpSession session = sender as TcpSession;
            //string msg = Encoding.UTF8.GetString(cmd.Data);
            //Console.WriteLine("sessionId:{0},cmdId:{1},msg:{2}", session.SessionId, cmd.CommondId, msg);
            var result = new byte[data.DataSize];
            Buffer.BlockCopy(data.Buffer, 0, result, 0, data.DataSize);
            //for (int i = 0; i < 50; i++)
            //{
            // sdata.Buffer = cmd.Data;
            // Thread.Sleep(4000);
            session.SendAsync(new SendData { Buffer = result });
            //}
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
