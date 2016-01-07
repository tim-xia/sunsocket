using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using SunSocket.Client;
using SunSocket.Core.Interface;
using SunSocket.Client.Interface;
using SunSocket.Client.Protocol;
using MsgPack.Serialization;
using System.IO;
using System.Diagnostics;

namespace TcpClient
{
    class MyClient : TcpClientSession
    {
        public MyClient(EndPoint server, ILoger loger) : base(server, 1024, loger)
        {

        }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            Received(session, dataBuffer);
        }
        public event EventHandler<IDynamicBuffer> Received;
    }
    class QueryClient : TcpClientSession
    {
        TaskCompletionSource<byte[]> tSource;
        CancellationTokenSource cancelSource;
        public QueryClient(EndPoint server, ILoger loger) : base(server, 1024, loger)
        {

        }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            byte[] result = new byte[dataBuffer.DataSize];
            Buffer.BlockCopy(dataBuffer.Buffer, 0, result,0, dataBuffer.DataSize);
            tSource.SetResult(result);
        }
        
        public async Task<byte[]> QueryAsync(byte[] data)
        {
            tSource = new TaskCompletionSource<byte[]>();
            base.SendAsync(data);
            cancelSource = new CancellationTokenSource(5000);
            tSource.Task.Wait(cancelSource.Token);
            return await tSource.Task;
        }
        public override void OnDisConnect(ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
    }
    class Program
    {
        static MyClient client;
        static QueryClient QClient;
        static Loger loger;
        static int receiveCount = 0;
        static int allCount = 100000;
        static Stopwatch sb = new Stopwatch();
        static void Main(string[] args)
        {
            CommonTest();
        }
        public static void AnswerTest()
        {
            loger = new Loger();
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            QClient = new QueryClient(endPoint, loger);
            //client = new MyClient(endPoint, loger);
            //client.PacketProtocol = new TcpClientPacketProtocol(1024, 1024 * 4);
            QClient.PacketProtocol = new TcpClientPacketProtocol(1024, 1024 * 4);
            QClient.Connect();
           // client.Connect();
            Stopwatch sw = new Stopwatch();
            Console.WriteLine("连接服务器成功");
            int i = 0;
            string c = Console.ReadLine();
            if (!string.IsNullOrEmpty(c))
                allCount = int.Parse(c);
            sw.Start();//开始记录时间
            while (i <= allCount)
            {
                i++;
                var data = Encoding.UTF8.GetBytes("测试数据kjfl发送大法师大法是大法师大法是否阿斯发达说" + i);
               var t = QClient.QueryAsync(data);
               // client.SendAsync(data);
                t.Wait();
                //if (t.IsCompleted)
                //    Console.WriteLine(Encoding.UTF8.GetString(t.Result));
                //else
                //    Console.WriteLine(i+":错误");
               // Thread.Sleep(10);
            }
            sw.Stop();
            Console.WriteLine("{0}次同步查询完成，运行时间：{1} 秒{2}毫秒", i, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
            Console.ReadLine();
        }
        public static void CommonTest()
        {
            loger = new Loger();
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            client = new MyClient(endPoint, loger);
            client.PacketProtocol = new TcpClientPacketProtocol(1024, 1024 * 4);
            client.Received += ReceiveCommond;
            client.Connect();

            Stopwatch sw = new Stopwatch();
            Console.WriteLine("连接服务器成功");
            int i = 0;
            string c = Console.ReadLine();
            if (!string.IsNullOrEmpty(c))
                allCount = int.Parse(c);
            sw.Start();//开始记录时间
            sb.Start();
            while (i <= allCount)
            {
                i++;
                var data = Encoding.UTF8.GetBytes("测试数据kjfl发送大法师大法是大法师大法是否阿斯发达说法是否大是大非阿斯顿飞啊的方式阿斯顿飞阿凡达是啊发送到啊发送方啊发送的发送方啊是否啊第三方啊是否啊是否的萨芬啊是否啊是否阿飞大师傅kdsfjlkasjdflkjasdfljaslkfdjlkasdfjlkajsdlk" + i);
                client.SendAsync(data);
            }
            sw.Stop();
            Console.WriteLine("发送{0}次数据完成，运行时间：{1} 秒{2}毫秒", i, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
            Console.ReadLine();
        }
        public static ITcpClientPacketProtocol GetProtocol()
        {
            return new TcpClientPacketProtocol(1024, 1024 * 4);
        }
        public static void ReceiveCommond(object sender, IDynamicBuffer data)
        {
            if (Interlocked.Increment(ref receiveCount) >= allCount)
            {
                sb.Stop();
                Console.WriteLine("接收{0}次数据完成，运行时间：{1} 秒{2}毫秒", allCount, sb.Elapsed.Seconds, sb.Elapsed.Milliseconds);
            }
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

