using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Interface;

namespace SunRpc.Server
{
    public interface IServerController
    {
        void Execute(ITcpSession session);
        ITcpSession Session { get; set; }
        ProxyFactory RpcFactory { get; set; }
        bool SingleInstance { get; }
    }
}
