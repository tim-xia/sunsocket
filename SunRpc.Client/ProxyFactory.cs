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
        IocContainer<IClentController> iocContainer;
        public ProxyFactory(ClientConfig config)
        {
            this.Config = config;
            iocContainer = new IocContainer<IClentController>();
            iocContainer.Load(config.BinPath);
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
            connect.TypeContainer = iocContainer;
            return connect;
        }
    }
}
