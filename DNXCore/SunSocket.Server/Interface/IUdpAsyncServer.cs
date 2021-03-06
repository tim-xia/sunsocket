﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core;
using SunSocket.Core.Session;
using SunSocket.Core.Protocol;
using System.Net.Sockets;
using System.Net;

namespace SunSocket.Server.Interface
{
    public interface IUdpAsyncServer
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 监听socket
        /// </summary>
        Socket ListenerSocket { get; set; }
        /// <summary>
        /// 开始接收数据
        /// </summary>
        void Start();
        /// <summary>
        /// 接收数据
        /// </summary>
        void Stop();
        void SendAsync(EndPoint endPoint, SendData cmd);
        /// <summary>
        /// 数据包提取完成事件
        /// </summary>
        event EventHandler<byte[]> OnReceived;
    }
}
