using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core.Interface;

namespace SunSocket.Client.Interface
{
    public interface ITcpClientSessionPool : IMonitorPool<string, ITcpClientSession>
    {
        ITcpClientPacketProtocol GetProtocal();
    }
}
