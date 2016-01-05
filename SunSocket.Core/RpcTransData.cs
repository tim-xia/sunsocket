using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunSocket.Core
{
    public class RpcTransData
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public List<byte[]> Arguments { get; set; }
    }
}
