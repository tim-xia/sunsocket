﻿using System;
using System.Net;
using SunSocket.Core.Protocol;
using SunSocket.Core.Interface;
using SunSocket.Client.Interface;

namespace SunSocket.Client
{
    public class AsyncClient : IAsyncClient
    {
        int bufferPoolSize, bufferSize;
        ILoger loger;
        public AsyncClient(int bufferPoolSize, int bufferSize, ILoger loger)
        {
            this.bufferPoolSize = bufferPoolSize;
            this.bufferSize = bufferSize;
            this.loger = loger;
        }
        public event EventHandler<ITcpClientSession> OnConnected;
        public event EventHandler<ReceiveCommond> OnReceived;
        public event EventHandler<ITcpClientSession> OnDisConnect;
        public void Connect(EndPoint remoteEndPoint) 
        {
            TcpClientSession session = new TcpClientSession(remoteEndPoint,bufferPoolSize,bufferSize,loger);
            session.OnReceived += OnReceived;
            session.OnConnected += OnConnected;
            session.OnDisConnect += OnDisConnect;
            session.Connect();
        }

        public void Disconnect(ITcpClientSession sesseion)
        {
            sesseion.DisConnect();
        }
    }
}
