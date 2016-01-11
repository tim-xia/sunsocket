using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunRpc.Core
{
    public interface IRpcInvoke
    {
        Task<T> Invoke<T>(string controller, string action, params object[] arguments);
        Task<object> Invoke(Type returnType, string controller, string action, params object[] arguments);
    }
}
