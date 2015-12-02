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
    public interface ITcpServer
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        TcpServerConfig Config { get; set; }
        /// <summary>
        /// 日志处理器
        /// </summary>
        ILoger Loger { get; set; }
        /// <summary>
        /// 监听socket
        /// </summary>
        Socket ListenerSocket { get; set; }
        /// <summary>
        /// 在线列表
        /// </summary>
        ConcurrentDictionary<string, ITcpSession> OnlineList { get;}
        /// <summary>
        /// 启动监听
        /// </summary>
        void Start();
        /// <summary>
        /// 停止监听
        /// </summary>
        void Stop();
        /// <summary>
        /// 开始接受请求
        /// </summary>
        /// <param name="acceptEventArgs">异步套接字操作</param>
        void StartAccept(SocketAsyncEventArgs acceptEventArgs);

        void OnReceived(ITcpSession session, IDynamicBuffer dataBuffer);
        /// <summary>
        /// 当连接请求通过后
        /// </summary>
        void OnConnected(ITcpSession session);
        /// <summary>
        /// 断开连接通知
        /// </summary>
        void OnDisConnect(ITcpSession session);
        /// <summary>
        /// 获取协议解析器
        /// </summary>
        /// <returns></returns>
        ITcpPacketProtocol GetProtocol();
    }
}
