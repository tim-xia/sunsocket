using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Client;
using Rpc.Interface.Client;

namespace Rpc.Client.Controller
{
    public class Caculator:ClientController,ICaculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public void BroadCast(string message)
        {
            Console.WriteLine(message);
        }

        public List<string> GetList()
        {
            return new List<string>() { "我的世界", "开始下雪", "啦啦啦" };
        }
    }
}
