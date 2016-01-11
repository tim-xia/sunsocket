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
