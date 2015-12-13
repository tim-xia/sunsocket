using System;
using SunSocket.Core.Session;
using System.Net.Sockets;
using System.Net;
using SunSocket.Core;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Session
{
    public class RUdpSession : IRUdpSession
    {
        PackageId idGenerator;
        public RUdpSession(EndPoint remoteEndPoint,IRUdpServer server)
        {
            this.idGenerator = new PackageId();
            endPoint = remoteEndPoint;
            Server = server;
        }
        private EndPoint endPoint;
        public EndPoint EndPoint
        {
            get {
                return endPoint;
            }
            set {
                idGenerator.Init();
                endPoint = value;
            }
        }
        public IRUdpSessionPool Pool
        {
            get;
            set;
        }
        public DataContainer SessionData
        {
            get; set;
        }
        public IRUdpServer Server
        {
            get;set;
        }

        public DateTime? ConnectDateTime
        {
            get;
            set;
        }

        public DateTime ActiveDateTime
        {
            get;
            set;
        }

        public void SendAsync(byte[] data)
        {
            Server.SendAsync(EndPoint, new SendData() {Id=idGenerator.NewId(),Data = data });
        }
        public void SendSucess(uint packageId)
        {

        }
        object closeLock = new object();
        public void DisConnect()
        {
            if (ConnectDateTime != null)
            {
                lock (closeLock)
                {
                    if (ConnectDateTime != null)
                    {
                        if (Pool != null)
                        {
                            Clear();
                            Pool.Push(this);
                        }
                        else
                        {
                            Dispose();
                        }
                    }
                }
            }
        }
        public void Clear()
        {
            endPoint = null;
            SessionData.Clear();//清理session数据
            idGenerator.Stop();
        }
        public void Dispose()
        {
            Clear();
        }
    }
}
