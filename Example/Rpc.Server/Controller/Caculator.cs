using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Server;
using SunRpc.Core.Controller;
using Rpc.Interface.Server;
using Rpc.Interface.Client;

namespace Rpc.Server.Controller
{
    public class Caculator:ServerController,ISCaculator
    {
        [Action]
        public int Add(int a, int b)
        {
            var r = Session.GetInstance<ICaculator>();
            var d = r.Add(a, b);
            //return a + b+d;
            return a + b;
        }
        [Action]
        public void BroadCast(string message)
        {
            foreach (var session in Session.Pool.ActiveList.Values)
            {
                if (session.SessionId != this.Session.SessionId)
                {
                    var r = Session.GetInstance<ICaculator>();
                    r.BroadCast(message);
                }
            }
        }
        [Action]
        public List<string> GetList()
        {
            return new List<string>() { "我的世界","开始下雪","啦啦啦"};
        }
    }
}
