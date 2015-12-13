using System;
using System.Net;
using System.Net.Sockets;
using SunSocket.Core.Interface;
using System.Collections.Concurrent;
using SunSocket.Server.Interface;
using SunSocket.Server.Session;
using SunSocket.Server.Protocol;
using SunSocket.Server.Config;
using SunSocket.Core.Protocol;

namespace SunSocket.Server
{
    public class RUdpServer : IRUdpServer
    {
        public uint ServerId
        {
            get
            {
                return Config.ServerId;
            }
        }

        /// <summary>
        /// 配置信息
        /// </summary>
       public RUdpServerConfig Config { get; set; }

        public IPool<SocketAsyncEventArgs> SocketArgsPool
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void SendAsync(EndPoint endPoint, SendData cmd)
        {
            throw new NotImplementedException();
        }
    }
}
