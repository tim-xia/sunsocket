using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunSocket.Core
{
    public class RpcReturnData
    {
        public int Id { get; set; }
        public List<byte[]> Values { get; set; }
    }
}
