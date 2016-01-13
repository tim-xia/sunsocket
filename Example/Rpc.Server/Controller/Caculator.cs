using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Server;
using Rpc.Interface.Server;
using Rpc.Interface.Client;

namespace Rpc.Server.Controller
{
    public class Caculator:ServerController,ISCaculator
    {
        int q = 1;
        public int Add(int a, int b)
        {
            int d = 10;
            q = d;
            //var r = RpcFactory.GetInstance<ICaculator>(Session, "Caculator");
            //var d = r.Add(a, b);
            //return a + b+d;
            return a + b;
        }

        public void BroadCast(string message)
        {
            foreach (var session in Session.Pool.ActiveList.Values)
            {
                if (session.SessionId != this.Session.SessionId)
                {
                    var r = RpcFactory.GetInstance<ICaculator>(session, "Caculator");
                    r.BroadCast(message);
                }
            }
        }

        public List<string> GetList()
        {
            return new List<string>() { "我的世界","开始下雪","啦啦啦"};
        }
    }
}
