﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SunRpc.Core
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class RpcCallData
    {
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public List<byte[]> Arguments { get; set; }
    }
}
