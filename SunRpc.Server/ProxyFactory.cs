using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Core;
using SunSocket.Server.Interface;

namespace SunRpc.Server
{
    public class ProxyFactory
    {
        public RpcServerConfig config;
        public ProxyFactory(RpcServerConfig config)
        {
            this.config = config;
            invokeDict = new ConcurrentDictionary<uint, RpcInvoke>();
        }
        public ConcurrentDictionary<uint, RpcInvoke> invokeDict;
        public RpcInvoke GetInvoke(uint SessionId)
        {
            RpcInvoke invoke;
            invokeDict.TryGetValue(SessionId, out invoke);
            return invoke;
        }
        public RpcProxy GetProxy<T>(ITcpSession session,string impName) where T : class
        {
            if (session == null) throw new Exception("session Can't be empty");
            var proxy = new RpcProxy(typeof(T), impName);
            proxy.RpcInvoke = GetInvoke(session.SessionId);
            return proxy;
        }
        public T GetInstance<T>(ITcpSession session, string impName) where T : class
        {
            if (session == null) throw new Exception("session Can't be empty");
            var proxy = GetProxy<T>(session, impName);
            return proxy.GetTransparentProxy() as T;
        }
    }
}
