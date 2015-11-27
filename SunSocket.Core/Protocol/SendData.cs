using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunSocket.Core.Protocol
{
    public class SendData
    {
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
    }
}
