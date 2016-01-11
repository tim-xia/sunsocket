using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Server;
using Rpc.Server.IClient;

namespace Rpc.Server.Controller
{
    public class Caculator:ServerController
    {
        public override bool SingleInstance
        {
            get
            {
                return base.SingleInstance;
            }
        }
        public int Add(int a, int b)
        {
            var r = RpcFactory.GetInstance<ICaculator>(Session, "Caculator");
            var d = r.Add(a, b);
            return a + b+d;
        }
        public List<string> GetList()
        {
            return new List<string>() { "我的世界","开始下雪","啦啦啦"};
        }
    }
}
