using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using SunSocket.Core.Session;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Interface
{
    public interface IAsyncServer
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
        /// 在线列表
        /// </summary>
        ConcurrentDictionary<string, ITcpSession> OnlineList { get;}
        /// <summary>
        /// 开始接受请求
        /// </summary>
        /// <param name="acceptEventArgs">异步套接字操作</param>
        void StartAccept(SocketAsyncEventArgs acceptEventArgs);
        /// <summary>
        /// 数据包提取完成事件
        /// </summary>
        event EventHandler<IDynamicBuffer> OnReceived;
        /// <summary>
        /// 当连接请求通过后
        /// </summary>
        event EventHandler<ITcpSession> OnConnected;
        /// <summary>
        /// 断开连接通知
        /// </summary>
        event EventHandler<ITcpSession> OnDisConnect;
    }
}
