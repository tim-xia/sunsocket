using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SunRpc.Core
{
    [ProtoContract(ImplicitFields =ImplicitFields.AllFields)]
    public class RpcReturnData
    {
        public int Id { get; set; }
        public byte[] Value { get; set; }
    }
}
