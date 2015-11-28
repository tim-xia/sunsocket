using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using SunSocket.Client;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Client.Interface;
using SunSocket.Client.Protocol;

namespace TcpClient
{
    class Program
    {
        static ITcpClientSession Session;
        static ConcurrentQueue<byte[]> CmdList = new ConcurrentQueue<byte[]>();
        static CancellationTokenSource cancelSource;
        static TcpClientSessionPool sessionPool;
        static Loger loger;
        static void Main(string[] args)
        {
            loger = new Loger();
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            sessionPool = new TcpClientSessionPool(endPoint, 1024, 1024 * 4, loger, GetProtocol);
            ITcpClientSession session = sessionPool.Pop();
            session.PacketProtocol = GetProtocol();
            session.OnReceived += ReceiveCommond;
            session.OnConnected += Connected;
            session.OnDisConnect += DisConnected;
            session.Connect();
            Console.ReadLine();
            short i = 0;
            while (i < 1)
            {
                i++;
                var data = Encoding.UTF8.GetBytes("测试数据kjfl发送大法师大法是大法师大法是否阿斯发达说法是否大是大非阿斯顿飞啊的方式阿斯顿飞阿凡达是啊发送到啊发送方啊发送的发送方啊是否啊第三方啊是否啊是否的萨芬啊是否啊是否阿飞大师傅kdsfjlkasjdflkjasdfljaslkfdjlkasdfjlkajsdlk" + i);
                try
                {
                    var result = SendAsync(data).Result;
                    Console.WriteLine(Encoding.UTF8.GetString(result));
                }
                catch
                {
                    Console.WriteLine("请求超时");
                }        
            }
            Console.WriteLine("处理完成");
            Console.ReadLine();
        }
        public static ITcpClientPacketProtocol GetProtocol()
        {
            return new TcpClientPacketProtocol(1024, 1024 * 4, loger);
        }
        static TaskCompletionSource<byte[]> tSource;
        public static async Task<byte[]> SendAsync(byte[] data)
        {
            tSource = new TaskCompletionSource<byte[]>();
            cancelSource = new CancellationTokenSource(5000);
            Session.SendAsync(new SendData() { Buffer = data });
            //if (!tSource.Task.IsCompleted)
            //{
            //    return null;
            //}
            tSource.Task.Wait(cancelSource.Token);
            return await tSource.Task;
        }
        public static void DisConnected(object sender, ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
        public static void Connected(object sender, ITcpClientSession session)
        {
            Console.WriteLine("连接成功，开始接受数据");
            Session = session;
        }
        public static void ReceiveCommond(object sender, byte[] data)
        {
            tSource.SetResult(data);
            //TcpClientSession session = sender as TcpClientSession;
            //string msg = Encoding.UTF8.GetString(cmd.Data);
            //li.Add(string.Format("sessionId:{0},cmdId:{1},msg:{2}", session.SessionId, cmd.CommondId, msg));
            //Console.WriteLine("sessionId:{0},cmdId:{1},msg:{2}", session.SessionId, cmd.CommondId, msg);
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

