using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core.Interface;

namespace SunSocket.Core.Protocol
{
    public class RUdpPackage
    {
        public uint Id { get; set; }
        public IFixedBuffer Buffer { get; set; }
    }
}
