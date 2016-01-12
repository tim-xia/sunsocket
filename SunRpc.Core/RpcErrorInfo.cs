using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SunRpc.Core
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class RpcErrorInfo
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }
}
