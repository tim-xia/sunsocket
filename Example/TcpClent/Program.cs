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
    class Program
    {
        static CancellationTokenSource cancelSource;
        static MyClient client;
        static Loger loger;
        static int receiveCount = 0;
        static int allCount = 1000000;
        static Stopwatch sb = new Stopwatch();
        static void Main(string[] args)
        {
            loger = new Loger();
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088);
            client = new MyClient(endPoint, loger);
            client.PacketProtocol = new TcpClientPacketProtocol(1024, 1024 * 4, loger);
            client.Received += ReceiveCommond;
            client.Connect();
            Console.ReadLine();
            int i = 0;
            Stopwatch sw = new Stopwatch();
            Console.WriteLine("连接服务器成功");
            sw.Start();//开始记录时间
            sb.Start();
            while (i <= allCount)
            {
                i++;
                var data = Encoding.UTF8.GetBytes("测试数据kjfl发送大法师大法是大法师大法是否阿斯发达说法是否大是大非阿斯顿飞啊的方式阿斯顿飞阿凡达是啊发送到啊发送方啊发送的发送方啊是否啊第三方啊是否啊是否的萨芬啊是否啊是否阿飞大师傅kdsfjlkasjdflkjasdfljaslkfdjlkasdfjlkajsdlk" + i);
                //try
                //{
                //    var result = SendAsync(data).Result;
                //   // Console.WriteLine(Encoding.UTF8.GetString(result));
                //}
                //catch
                //{
                //    Console.WriteLine("请求超时");
                //}
                Send(data);        
            }
            sw.Stop();//结束记录时间
                      //Console.WriteLine("单连接{0}次同步请求的运行时间：{1} 秒{2}毫秒", i, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
            Console.WriteLine("发送{0}次数据完成，运行时间：{1} 秒{2}毫秒", i, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
            Console.WriteLine();
            Console.WriteLine("发送完成");
            Console.ReadLine();
        }
        public static ITcpClientPacketProtocol GetProtocol()
        {
            return new TcpClientPacketProtocol(1024, 1024 * 4, loger);
        }
        static TaskCompletionSource<byte[]> tSource;
        public static void Send(byte[] data)
        {
            client.SendAsync(new SendData() { Data = data });
        }
        public static async Task<byte[]> SendAsync(byte[] data)
        {
            tSource = new TaskCompletionSource<byte[]>();
            cancelSource = new CancellationTokenSource(5000);
            client.SendAsync(new SendData() { Data = data });
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
        }
        public static void ReceiveCommond(object sender, IDynamicBuffer data)
        {
            if (Interlocked.Increment(ref receiveCount) > allCount)
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

