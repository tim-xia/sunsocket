using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Core.Ioc;
using SunSocket.Core;

namespace SunRpc.Client
{
    public class ProxyFactory
    {
        RpcContainer<IClentController> rpcContainer;
        public ProxyFactory(ClientConfig config)
        {
            this.Config = config;
            rpcContainer = new RpcContainer<IClentController>();
            rpcContainer.Load(config.BinPath);
            sessionId = new SessionId(1);
        }
        SessionId sessionId;
        public ClientConfig Config
        {
            get;
            set;
        }
        public Connect GetConnect()
        {
            var connect = new Connect(Config);
            connect.SessionId = sessionId.NewId();
            connect.Connect();
            connect.RpcContainer = rpcContainer;
            return connect;
        }
    }
}
