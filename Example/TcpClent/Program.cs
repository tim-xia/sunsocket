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

namespace TcpClient
{
    class Program
    {
        static ITcpClientSession Session;
        static ConcurrentQueue<ReceiveCommond> CmdList = new ConcurrentQueue<ReceiveCommond>();
        static CancellationTokenSource cancelSource;
        static void Main(string[] args)
        {
            AsyncClient client = new AsyncClient(1024, 1024 * 4, new Loger());
            client.OnReceived += ReceiveCommond;
            client.OnConnected += Connected;
            client.OnDisConnect += DisConnected;
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088));
            Console.ReadLine();
            short i = 0;
            while (i < 5)
            {
                Task.Delay(1).Wait();
                i++;
                var data = Encoding.UTF8.GetBytes("测试数据" + i);
                try
                {
                    var result = SendAsync(data).Result;
                    //Console.WriteLine(Encoding.UTF8.GetString(result.Data));
                }
                catch
                {
                    Console.WriteLine("请求超时");
                }        
            }
            Console.WriteLine("处理完成");
            Console.ReadLine();
        }
        static TaskCompletionSource<ReceiveCommond> tSource;
        public static async Task<ReceiveCommond> SendAsync(byte[] data)
        {
            tSource = new TaskCompletionSource<ReceiveCommond>();
            cancelSource = new CancellationTokenSource(5000);
            Session.SendAsync(new SendCommond() { CommondId = 1, Buffer = data });
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
        public static void ReceiveCommond(object sender, ReceiveCommond cmd)
        {
            tSource.SetResult(cmd);
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

