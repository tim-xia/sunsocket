using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Server.Ioc;
using SunRpc.Server.Controller;

namespace Rpc.Server.Controller
{
    public class Caculator:ControllerBase
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
        public List<string> GetList()
        {
            return new List<string>() { "我的世界","开始下雪","啦啦啦"};
        }
    }
}
