using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Interface;

namespace SunRpc.Server
{
    public abstract class ServerController : IServerController
    {
        public ProxyFactory RpcFactory
        {
            get;
            set;
        }

        public ITcpSession Session
        {
            get;
            set;
        }

        public virtual bool SingleInstance
        {
            get
            {
                return false;
            }
        }

        public virtual void Execute(ITcpSession session)
        {

        }
    }
}
