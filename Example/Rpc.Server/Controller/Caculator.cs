using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Server.Ioc;
using SunRpc.Server.Controller;

namespace Rpc.Server.Controller
{
    [RPC("Caculator")]
    public class Caculator:ControllerBase
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
