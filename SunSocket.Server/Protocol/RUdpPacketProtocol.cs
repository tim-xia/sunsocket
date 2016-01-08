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
    public class RUdpPacketProtocol : IRUdpPacketProtocol
    {
        bool NetByteOrder = false;
        PackageId idGenerator;
        static int uIntByteLength = sizeof(uint);
        static int dbUIntByteLength = uIntByteLength * 2;
        private object clearLock = new object();
        private int alreadyReceivePacketLength, needReceivePacketLenght;
        private IFixedBuffer InterimPacketBuffer;
        //数据接收缓冲器队列
        private Queue<IFixedBuffer> ReceiveBuffers;
        //数据发送缓冲器
        public IFixedBuffer SendBuffer { get; set; }
        private IDynamicBuffer ReceiveDataBuffer { get; set; }
        IRUdpSession _session;
        public IRUdpSession Session
        {
            get { return _session; }
            set {
                _session = value;
                idGenerator = new PackageId();
                SendBuffer = new FixedBuffer(value.Pool.RUdpServer.Config.BufferSize);
                ReceiveDataBuffer = new DynamicBuffer(value.Pool.RUdpServer.Config.BufferSize);
            }
        }

        private SendData NoComplateCmd = null;//未完全发送指令
        bool isSend = false;//发送状态
        private ConcurrentQueue<SendData> sendDataQueue = new ConcurrentQueue<SendData>();//指令发送队列
        public RUdpPacketProtocol()
        {
            ReceiveBuffers = new Queue<IFixedBuffer>();
        }
        uint prePackageId;
        private object receiveLock = new object();
        public bool Receive(SocketAsyncEventArgs e)
        {
            var receiveBuffer = e.Buffer;
            int offset = e.Offset;
            int count = e.BytesTransferred;
            if (count == uIntByteLength)
            {
                if (BitConverter.ToUInt32(receiveBuffer, offset) == SendId)
                {
                    isSendSucess = true;
                }
                return true;
            }
            else if (count > dbUIntByteLength)
            {
                uint length = BitConverter.ToUInt32(receiveBuffer, offset);
                offset += uIntByteLength;
                count -= uIntByteLength;
                if (length == count)
                {
                    uint packageId = BitConverter.ToUInt32(receiveBuffer, offset);
                    offset += uIntByteLength;
                    count -= uIntByteLength;
                    lock(receiveLock)
                    {
                        if (packageId > prePackageId || prePackageId - packageId > 983040000)
                        {
                            Session.CommonSendAsync(BitConverter.GetBytes(packageId));//发送接收成功通知
                            RUdpBuffer receiveBufferObj = new RUdpBuffer();
                            receiveBufferObj.FixedBuffer = e.UserToken as IFixedBuffer;
                            receiveBufferObj.Offset = offset;
                            receiveBufferObj.Count = count;
                            ProcessReceive(receiveBufferObj);
                            var buffer = Session.Pool.RUdpServer.BufferPool.Pop();
                            e.SetBuffer(buffer.Buffer, 0, buffer.Buffer.Length);
                            e.UserToken = buffer;
                            prePackageId = packageId;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private ConcurrentQueue<RUdpBuffer> receiveQueue = new ConcurrentQueue<RUdpBuffer>();
        private bool isReceive = false;
        public void ProcessReceive(RUdpBuffer buffer)
        {
            receiveQueue.Enqueue(buffer);
            if (!isReceive)
            {
                isReceive = true;
                Task.Factory.StartNew(() => { ProcessReceiveQueue(); });//启用独立线程处理接收到的数据
            }
        }
 
        private void ProcessReceiveQueue()
        {
            RUdpBuffer buffer;
            if (!receiveQueue.TryDequeue(out buffer))
            {
                isReceive=false;
                return;
            }
            byte[] receiveBuffer = buffer.FixedBuffer.Buffer;
            int offset = buffer.Offset;
            int count = buffer.Count;
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
                        getLenght = cacheBuffer.DataSize - uIntByteLength;
                        ReceiveDataBuffer.WriteBuffer(cacheBuffer.Buffer, uIntByteLength, getLenght);
                        Session.Pool.FixedBufferPool.Push(cacheBuffer);
                        while (ReceiveBuffers.Count > 0)
                        {
                            var popBuffer = ReceiveBuffers.Dequeue();
                            ReceiveDataBuffer.WriteBuffer(popBuffer.Buffer, 0, popBuffer.DataSize);
                            getLenght += popBuffer.DataSize;
                            Session.Pool.FixedBufferPool.Push(popBuffer);
                        }
                        var needLenght = needReceivePacketLenght - getLenght - uIntByteLength;
                        ReceiveDataBuffer.WriteBuffer(receiveBuffer, offset, needLenght);
                        offset += needLenght;
                        count -= needLenght;
                        //触发获取指令事件
                        ReceiveData();
                        //清理合包数据
                        needReceivePacketLenght = 0; alreadyReceivePacketLength = 0;
                    }
                }
                else
                {
                    if (InterimPacketBuffer != null && alreadyReceivePacketLength == 0)
                    {
                        var diff = uIntByteLength - InterimPacketBuffer.DataSize;
                        InterimPacketBuffer.WriteBuffer(receiveBuffer, offset, diff);
                        offset += diff;
                        count -= diff;
                        var packetLength = BitConverter.ToInt32(InterimPacketBuffer.Buffer, 0);
                        if (NetByteOrder)
                            packetLength = IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
                        if (count >= packetLength)
                        {
                            Session.Pool.FixedBufferPool.Push(InterimPacketBuffer);
                            InterimPacketBuffer = null;
                            ReceiveDataBuffer.WriteBuffer(receiveBuffer, offset, packetLength);
                            ReceiveData();
                            offset += packetLength;
                            count -= packetLength;
                        }
                        else
                        {
                            needReceivePacketLenght = packetLength + uIntByteLength;
                            alreadyReceivePacketLength = uIntByteLength;
                        }
                    }
                }
                if (needReceivePacketLenght > 0)
                {
                    while (count > 0)//遍历把数据放入缓冲器中
                    {
                        if (InterimPacketBuffer == null)
                        {
                            InterimPacketBuffer = Session.Pool.FixedBufferPool.Pop();
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
                    if (count >= uIntByteLength)
                    {
                        //按照长度分包
                        int packetLength = BitConverter.ToInt32(receiveBuffer, offset); //获取包长度
                        if (NetByteOrder)
                            packetLength = IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序

                        if (packetLength < 0)
                        {
                            var ddd = packetLength;
                        }
                        if ((count - uIntByteLength) >= packetLength) //如果数据包达到长度则马上进行解析
                        {
                            ReceiveDataBuffer.WriteBuffer(receiveBuffer, offset + uIntByteLength, packetLength);
                            //触发获取指令事件
                            ReceiveData();
                            int processLenght = packetLength + uIntByteLength;
                            offset += processLenght;
                            count -= processLenght;
                        }
                        else
                        {
                            needReceivePacketLenght = packetLength + uIntByteLength;//记录当前包总共需要多少的数据
                        }
                    }
                    else
                    {
                        if (InterimPacketBuffer == null)
                        {
                            InterimPacketBuffer = Session.Pool.FixedBufferPool.Pop();
                        }
                        InterimPacketBuffer.WriteBuffer(receiveBuffer, offset, count);
                        count = 0;
                    }
                }
            }
            Session.Pool.RUdpServer.BufferPool.Push(buffer.FixedBuffer);//把缓冲块放入池中
            ProcessReceiveQueue();
        }
        public void ReceiveData()
        {
            Session.Pool.RUdpServer.OnReceived(Session, ReceiveDataBuffer);
            ReceiveDataBuffer.Clear();//清空数据接收器缓存
        }
        object lockObj = new object();
        public void SendAsync(byte[] data)
        {
            SendData sendData = new SendData() { Data = data, Offset = 0 };
            sendDataQueue.Enqueue(sendData);
            if (!isSend)
            {
                lock(lockObj)
                {
                    if (!isSend)
                    {
                        isSend = true;
                        if (Session.EndPoint != null)
                        {
                            SendProcess();
                        }
                        else
                        {
                            Session.DisConnect();
                        }
                    }
                }
            }
        }
        TaskCompletionSource<bool> sendTask;
        bool isSendSucess;
        int timerMillisecond = 100;
        int retryTimes;
        public void SendProcess()
        {
            SendBuffer.DataSize=dbUIntByteLength; //清除已发送的包
            int dataBufferSize = SendBuffer.Buffer.Length - dbUIntByteLength;
            int surplus = dataBufferSize;
            while (sendDataQueue.Count > 0)
            {
                if (NoComplateCmd != null)
                {
                    int noComplateLength = NoComplateCmd.Data.Length - NoComplateCmd.Offset;
                    if (noComplateLength <= surplus)
                    {
                        SendBuffer.WriteBuffer(NoComplateCmd.Data, NoComplateCmd.Offset, noComplateLength);
                        surplus -= noComplateLength;
                        NoComplateCmd = null;
                    }
                    else
                    {
                        SendBuffer.WriteBuffer(NoComplateCmd.Data, NoComplateCmd.Offset, surplus);
                        NoComplateCmd.Offset += surplus;
                        surplus -= surplus;
                        break;//跳出当前循环
                    }
                }
                if (surplus >= uIntByteLength)
                {
                    SendData data;
                    if (sendDataQueue.TryDequeue(out data))
                    {
                        var PacketAllLength = data.Data.Length + uIntByteLength;
                        if (PacketAllLength <= surplus)
                        {
                            SendBuffer.WriteInt(data.Data.Length, false); //写入总大小
                            SendBuffer.WriteBuffer(data.Data); //写入命令内容
                            surplus -= PacketAllLength;
                        }
                        else
                        {
                            SendBuffer.WriteInt(data.Data.Length, false); //写入总大小
                            surplus -= uIntByteLength; ;
                            if (surplus > 0)
                            {
                                SendBuffer.WriteBuffer(data.Data, data.Offset, surplus); //写入命令内容
                                data.Offset = surplus;
                            }
                            NoComplateCmd = data;//把未全部发送指令缓存
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            if (surplus < dataBufferSize)
            {
                if (Session.EndPoint != null)
                {
                    Send();
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
        uint SendId;
        public async Task Send()
        {
            SendId = idGenerator.NewId();
            isSendSucess = false;
            retryTimes = 1;
            Buffer.BlockCopy(BitConverter.GetBytes(SendBuffer.DataSize - uIntByteLength), 0, SendBuffer.Buffer, 0, uIntByteLength);
            Buffer.BlockCopy(BitConverter.GetBytes(SendId), 0, SendBuffer.Buffer, uIntByteLength, uIntByteLength);
            Session.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataSize);
            sendTask = new TaskCompletionSource<bool>();
            if (!Session.Pool.RUdpServer.ListenerSocket.SendToAsync(Session.SendEventArgs)) SendCompleted(null, Session.SendEventArgs);
            var r = await sendTask.Task;
            if (r)
            {
                await Task.Delay(5);
                if (!isSendSucess)
                {
                    while (true)
                    {
                        if (isSendSucess)
                        {
                            SendCompleted(true);
                        }
                        if (retryTimes >= Session.Pool.RUdpServer.Config.SendMaxRetryTimes)
                            SendCompleted(false);//发送次数超过尝试最大次数
                        await Task.Delay(timerMillisecond * retryTimes);
                        sendTask = new TaskCompletionSource<bool>();
                        if (!Session.Pool.RUdpServer.ListenerSocket.SendToAsync(Session.SendEventArgs)) SendCompleted(null, Session.SendEventArgs);
                        var rTwo = await sendTask.Task;
                        if (!rTwo)
                        {
                            SendCompleted(false);
                        }
                        retryTimes++;
                    }
                }
                else
                {
                    SendCompleted(true);
                }
            }
            else
            {
                SendCompleted(false);
            }
        }
        public void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
                sendTask.SetResult(true);
            else
                sendTask.SetResult(false);
        }
        public void SendCompleted(bool sucess)
        {
            if (sucess)
                SendProcess();
            else
                Session.DisConnect();//发送失败断开连接
        }
        public void Clear()
        {
            lock (clearLock)
            {
                isSend = false;
                if (sendDataQueue.Count > 0)
                {
                    SendData cmd;
                    if (!sendDataQueue.TryDequeue(out cmd))
                    {
                        SpinWait spinWait = new SpinWait();
                        while (sendDataQueue.TryDequeue(out cmd))
                        {
                            spinWait.SpinOnce();
                        }
                    }
                }
                if (InterimPacketBuffer != null)
                {
                    Session.Pool.FixedBufferPool.Push(InterimPacketBuffer);
                    InterimPacketBuffer = null;
                }
                while (ReceiveBuffers.Count > 0)
                {
                    var packetBuffer = ReceiveBuffers.Dequeue();
                    Session.Pool.FixedBufferPool.Push(packetBuffer);
                }
            }
            SendBuffer.Clear();
            NoComplateCmd = null;
            alreadyReceivePacketLength = 0;
            needReceivePacketLenght = 0;
        }
    }
    public class RUdpBuffer
    {
        public IFixedBuffer FixedBuffer { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
