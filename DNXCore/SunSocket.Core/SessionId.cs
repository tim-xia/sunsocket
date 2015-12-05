﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SunSocket.Core
{
    public class SessionId
    {
        private static uint serverMask = 0xfff00000;
        private static uint idMask = 0xfffff;
        private long serverId;
        private int id = 0;
        public SessionId(long serverId)
        {
            if (serverId > 4095||serverId<1) throw new Exception("服务器编号不能大于4095和小于1");
            this.serverId = serverId;
        }
        public long NewId()
        {
            int newId = Interlocked.Increment(ref id);
            if (newId > 1048575) throw new Exception("session编号不能超过1048575");
            long result = ((serverId << 20) & serverMask) | (newId & idMask);
            return result;
        }
        public static long GetServerId(long id)
        {
            return id >> 20;
        }
    }
}
