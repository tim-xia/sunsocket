using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Core;

namespace Rpc.Interface.Server
{
    public interface ISCaculator:IBase
    {
        int Add(int a, int b);
        void BroadCast(string message);
    }
}
