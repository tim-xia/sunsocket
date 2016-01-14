using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Core;
using SunSocket.Server.Interface;

namespace SunRpc.Server
{
    public static class SessionExtensions
    {
        public static RpcProxy GetProxy<T>(this ITcpSession session, string impName = null) where T : class
        {
            if (session == null) throw new Exception("session Can't be empty");
            if (string.IsNullOrEmpty(impName)) impName = typeof(T).Name;
            var proxy = new RpcProxy(typeof(T), impName);
            ProxyFactory fac= session.SessionData.Get("proxyfactory") as ProxyFactory;
            proxy.RpcInvoke = fac.GetInvoke(session.SessionId);
            return proxy;
        }
        public static T GetInstance<T>(this ITcpSession session, string impName = null) where T : class
        {
            if (session == null) throw new Exception("session Can't be empty");
            var proxy = GetProxy<T>(session, impName);
            return proxy.GetTransparentProxy() as T;
        }
    }
}
