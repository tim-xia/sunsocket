using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Interface;

namespace SunRpc.Server.Controller
{
    public interface IController
    {
        void Execute(ITcpSession session);
    }
}
