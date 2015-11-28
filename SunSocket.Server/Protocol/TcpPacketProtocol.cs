using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SunSocket.Core;
using SunSocket.Core.Buffer;
using SunSocket.Core.Protocol;
using SunSocket.Core.Interface;
using SunSocket.Server.Interface;

namespace SunSocket.Server.Protocol
{
    public class TcpPacketProtocol : ITcpPacketProtocol
    {
       
        bool NetByteOrder = false;
        static int intByteLength = sizeof(int);
        private object clearLock = new object();
        ILoger loger;
        //缓冲器池
        private static FixedBufferPool BufferPool;
        private int alreadyReceivePacketLength, needReceivePacketLenght;
        private IFixedBuffer InterimPacketBuffer;
        //数据接收缓冲器队列
        private Queue<IFixedBuffer> ReceiveBuffers;
        //数据发送缓冲器
        public IFixedBuffer SendBuffer { get; set; }

        public ITcpSession Session
        {
            get;
            set;
        }

        private SendData NoComplateCmd = null;//未完全发送指令
        bool isSend = false;//发送状态
        private ConcurrentQueue<SendData> cmdQueue = new ConcurrentQueue<SendData>();//指令发送队列
        public TcpPacketProtocol(int bufferSize,int bufferPoolSize,ILoger loger)
        {
            this.loger = loger;
            if(BufferPool==null)
                BufferPool = new FixedBufferPool(bufferPoolSize, bufferSize);
            ReceiveBuffers = new Queue<IFixedBuffer>();
            SendBuffer = new FixedBuffer(bufferPoolSize);
        }
        public bool ProcessReceiveBuffer(byte[] receiveBuffer, int offset, int count)
        {
            while (count > 0)
            {
                if (needReceivePacketLenght > 0 && alreadyReceivePacketLength + count >= needReceivePacketLenght)//说明包已获取完成
                {
                    if (InterimPacketBuffer != null)
                    {
                        ReceiveBuffers.Enqueue(InterimPacketBuffer);
                        InterimPacketBuffer = null;
                    }
                    if (ReceiveBuffers.Count > 0)
                    {
                        int getLenght = 0;//已取出数据

                        var cacheBuffer = ReceiveBuffers.Dequeue();
                        int cachePacketLength = BitConverter.ToInt32(cacheBuffer.Buffer, 0); //获取包长度
                        var data = new byte[cachePacketLength];
                        getLenght = cacheBuffer.DataSize - intByteLength;
                        Buffer.BlockCopy(cacheBuffer.Buffer, intByteLength, data, 0, getLenght);
                        BufferPool.Push(cacheBuffer);
                        while (ReceiveBuffers.Count > 0)
                        {
                            var popBuffer = ReceiveBuffers.Dequeue();
                            Buffer.BlockCopy(popBuffer.Buffer, 0, data, getLenght, popBuffer.DataSize);
                            getLenght += popBuffer.DataSize;
                            BufferPool.Push(popBuffer);
                        }
                        var needLenght = needReceivePacketLenght - getLenght - intByteLength;
                        Buffer.BlockCopy(receiveBuffer, offset, data, getLenght, needLenght);
                        offset += needLenght;
                        count -= needLenght;
                        //触发获取指令事件
                        Session.Server.ReceiveData(Session, data);
                        //清理合包数据
                        needReceivePacketLenght = 0; alreadyReceivePacketLength = 0;
                    }
                }
                if (needReceivePacketLenght > 0)
                {
                    while (count > 0)//遍历把数据放入缓冲器中
                    {
                        if (InterimPacketBuffer == null)
                        {
                            InterimPacketBuffer = BufferPool.Pop();
                        }
                        var surpos = InterimPacketBuffer.Buffer.Length - InterimPacketBuffer.DataSize;//中间buffer剩余空间
                        if (count > surpos)
                        {
                            InterimPacketBuffer.WriteBuffer(receiveBuffer, offset, surpos);
                            ReceiveBuffers.Enqueue(InterimPacketBuffer);
                            InterimPacketBuffer = null;
                            alreadyReceivePacketLength += surpos;//记录已接收的数据
                            offset += surpos;
                            count -= surpos;
                        }
                        else
                        {
                            InterimPacketBuffer.WriteBuffer(receiveBuffer, offset, count);
                            alreadyReceivePacketLength += count;//记录已接收的数据
                            count = 0;
                        }
                    }
                }
                else
                {
                    if (count > 0)
                    {
                        //按照长度分包
                        int packetLength = BitConverter.ToInt32(receiveBuffer, offset); //获取包长度
                        if (NetByteOrder)
                            packetLength = IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
                        if ((count - intByteLength) >= packetLength) //如果数据包达到长度则马上进行解析
                        {
                            var data = new byte[packetLength];
                            Buffer.BlockCopy(receiveBuffer, offset + intByteLength, data, 0, packetLength);
                            //触发获取指令事件
                            Session.Server.ReceiveData(Session, data);
                            int processLenght = packetLength + intByteLength;
                            offset += processLenght;
                            count -= processLenght;
                        }
                        else
                        {
                            needReceivePacketLenght = packetLength + intByteLength;//记录当前包总共需要多少的数据
                        }
                    }
                }
            }
            return count == 0;
        }
        object lockObj = new object();
        public bool SendAsync(SendData cmd)
        {
            cmdQueue.Enqueue(cmd);
            if (!isSend)
            {
                lock(lockObj)
                {
                    if (!isSend)
                    {
                        isSend = true;
                        if (Session.ConnectSocket != null)
                        {
                            Task.Run(() =>
                            {
                                SendProcess();
                            });
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public void SendProcess()
        {
            int surplus = SendBuffer.Buffer.Length;
            while (cmdQueue.Count > 0)
            {
                if (NoComplateCmd != null)
                {
                    int noComplateLength = NoComplateCmd.Buffer.Length - NoComplateCmd.Offset;
                    if (noComplateLength <= SendBuffer.Buffer.Length)
                    {
                        SendBuffer.WriteBuffer(NoComplateCmd.Buffer, NoComplateCmd.Offset, noComplateLength);
                        surplus -= noComplateLength;
                        NoComplateCmd = null;
                    }
                    else
                    {
                        SendBuffer.WriteBuffer(NoComplateCmd.Buffer, NoComplateCmd.Offset, SendBuffer.Buffer.Length);
                        NoComplateCmd.Offset += SendBuffer.Buffer.Length;
                        surplus -= SendBuffer.Buffer.Length;
                        break;//跳出当前循环
                    }
                }
                if (surplus >= intByteLength)
                {
                    SendData data;
                    if (cmdQueue.TryDequeue(out data))
                    {
                        var PacketAllLength = data.Buffer.Length + intByteLength;
                        if (PacketAllLength <= surplus)
                        {
                            SendBuffer.WriteInt(data.Buffer.Length, false); //写入总大小
                            SendBuffer.WriteBuffer(data.Buffer); //写入命令内容
                            surplus -= PacketAllLength;
                        }
                        else
                        {
                            SendBuffer.WriteInt(data.Buffer.Length, false); //写入总大小
                            surplus -= data.Buffer.Length;
                            if (surplus > 0)
                            {
                                SendBuffer.WriteBuffer(data.Buffer, data.Offset, surplus); //写入命令内容
                                data.Offset = surplus;
                            }
                            NoComplateCmd = data;//把未全部发送指令缓存
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            if (surplus < SendBuffer.Buffer.Length)
            {
                Session.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataSize);
                if (Session.ConnectSocket != null)
                {
                    bool willRaiseEvent = Session.ConnectSocket.SendAsync(Session.SendEventArgs);
                    if (!willRaiseEvent)
                    {
                        Session.SendComplate();
                    }
                }
                else
                {
                    isSend = false;
                }
            }
            else
            {
                isSend = false;
            }
        }
        public void Clear()
        {
            lock (clearLock)
            {
                isSend = false;
                if (cmdQueue.Count > 0)
                {
                    SendData cmd;
                    while (cmdQueue.TryDequeue(out cmd))
                    {
                    }
                }
                if (InterimPacketBuffer != null)
                {
                    BufferPool.Push(InterimPacketBuffer);
                    InterimPacketBuffer = null;
                }
                while (ReceiveBuffers.Count > 0)
                {
                    var packetBuffer = ReceiveBuffers.Dequeue();
                    BufferPool.Push(packetBuffer);
                }
            }
            SendBuffer.Clear();
            NoComplateCmd = null;
            alreadyReceivePacketLength = 0;
            needReceivePacketLenght = 0;
        }
    }
}
