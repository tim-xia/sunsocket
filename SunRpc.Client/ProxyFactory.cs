using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunRpc.Client
{
    public class ProxyFactory
    {
        public ProxyFactory(ClientConfig config)
        {
            this.Config = config;
            ConnPool = new ConnectPool(config);
        }
        public ConnectPool ConnPool
        {
            get;
            set;
        }
        public ClientConfig Config
        {
            get;
            set;
        }
        public RpcProxy GetProxy(Type type, string impName)
        {
            var proxy = new RpcProxy(type, impName);
            proxy.Factory = this;
            return proxy;
        }
    }
}
