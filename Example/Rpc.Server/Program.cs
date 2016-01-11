using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server;
using SunSocket.Core.Interface;
using SunSocket.Server.Config;
using SunRpc.Server;

namespace Rpc.Server
{
    class Program
    {
        static Loger loger = new Loger();
        static void Main(string[] args)
        {
            RpcServerConfig configOne = new RpcServerConfig { ServerId = 1, Name = "one", IP = "127.0.0.1", Port = 8088, BufferSize = 1024, MaxFixedBufferPoolSize = 1024 * 4, MaxConnections = 8000 };
            configOne.BinPath = AppDomain.CurrentDomain.BaseDirectory;
            RpcServer listener = new RpcServer(configOne, loger);
            listener.Start();
            Console.WriteLine("服务区启动成功");
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
