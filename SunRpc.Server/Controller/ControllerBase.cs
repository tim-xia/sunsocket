using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Interface;

namespace SunRpc.Server.Controller
{
    public abstract class ControllerBase : IController
    {
        public virtual void Execute(ITcpSession session)
        {
            
        }
    }
}
