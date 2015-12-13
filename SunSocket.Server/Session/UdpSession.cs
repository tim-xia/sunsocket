﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;

namespace SunSocket.Server.Session
{
    public class UdpSession : IUdpSession
    {
        public UdpSession(EndPoint remoteEndPoint,IUdpServer server)
        {
            RemoteEndPoint = remoteEndPoint;
            Server = server;
        }

        public EndPoint RemoteEndPoint
        {
            get;set;
        }

        public IUdpServer Server
        {
            get;set;
        }

        public void SendAsync(byte[] data)
        {
            Server.SendAsync(RemoteEndPoint, new SendData() { Data = data });
        }
    }
}
