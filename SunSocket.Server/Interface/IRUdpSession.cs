using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace SunSocket.Server.Interface
{
   public interface IRUdpSession
    {
        IRUdpServer Server { get; set; }
        EndPoint EndPoint { get; set; }
        void SendAsync(byte[] data);
    }
}
