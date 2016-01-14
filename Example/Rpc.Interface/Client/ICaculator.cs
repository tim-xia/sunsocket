using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Core;

namespace Rpc.Interface.Client
{
    public interface ICaculator:IBase
    {
        int Add(int a, int b);
        void BroadCast(string message);
        List<string> GetList();
    }
}
