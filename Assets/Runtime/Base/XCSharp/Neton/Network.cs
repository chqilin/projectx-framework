using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ProjectX
{
    public class NetworkClient
    {
        public int SendTimeout = 0;
        public int RecvTimeout = 0;
        public int BufferSize = 8192;
        public int MemorySize = 8192;

        public delegate void ProcessHandler();
        public delegate void FailureHandler(SocketError error);
        public delegate void RawdataHandler(byte[] data, int offset, int count);
        public delegate void MessageHandler(DatagramMessage message);
        public ProcessHandler OnConnSuccess;
        public FailureHandler OnConnFailure;
        public ProcessHandler OnSendSuccess;
        public FailureHandler OnSendFailure;
        public RawdataHandler OnSendRawdata;
        public MessageHandler OnSendMessage;
        public ProcessHandler OnRecvSuccess;
        public FailureHandler OnRecvFailure;
        public RawdataHandler OnRecvRawdata;
        public MessageHandler OnRecvMessage;

        Socket mClientSocket;
        IPEndPoint mLocalEndPoint;
        IPEndPoint mRemoteEndPoint;
        byte[] mRecvBuffer;
        MemoryStream mRecvMemory;
        long mMemoryPosition;
        DatagramManager mDatagramManager;

        #region Constructor
        public NetworkClient()
        {
            this.mClientSocket = null;
            this.mRemoteEndPoint = null;
            this.mRecvBuffer = new byte[this.BufferSize];
            this.mRecvMemory = new MemoryStream();
            this.mMemoryPosition = 0;
            this.mDatagramManager = new DatagramManager();
        }
        #endregion

        #region Public Methods
        public void RegisterMessage<T>(ushort id) where T : DatagramMessage
        {
            this.mDatagramManager.RegisterMessage(typeof(T), id);
        }
        public void UnregisterMessage<T>() where T : DatagramMessage
        {
            this.mDatagramManager.UnregisterMessage(typeof(T));
        }
        public void ClearMessages()
        {
            this.mDatagramManager.ClearMessages();
        }

        public void Connect(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            this.mLocalEndPoint = localEndPoint;
            this.mRemoteEndPoint = remoteEndPoint;
            this.Connect();
        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            this.mLocalEndPoint = null;
            this.mRemoteEndPoint = remoteEndPoint;
            this.Connect();
        }

        public void Connect(string localAddr, int localPort, string remoteAddr, int remotePort)
        {
            this.mLocalEndPoint = new IPEndPoint(IPAddress.Parse(localAddr), localPort);
            this.mRemoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort);
            this.Connect();
        }

        public void Connect(string remoteAddr, int remotePort)
        {
            this.mLocalEndPoint = null;
            this.mRemoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort);
            this.Connect();
        }

        public void Connect()
        {
            if (this.mRemoteEndPoint == null)
                throw new NullReferenceException("RemoteEndPoint is null");

            this.Disconnect();

            this.mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (this.RecvTimeout > 0)
                this.mClientSocket.ReceiveTimeout = this.RecvTimeout;
            if (this.SendTimeout > 0)
                this.mClientSocket.SendTimeout = this.SendTimeout;
            if (this.mLocalEndPoint != null)
                this.mClientSocket.Bind(this.mLocalEndPoint);

            try
            {
                this.mClientSocket.Connect(this.mRemoteEndPoint);
                this.HandleProcess(this.OnConnSuccess);
            }
            catch(Exception e)
            {
                this.HandleFailure(this.OnConnFailure, SocketError.SocketError);
            }
        }

        public void Disconnect()
        {
            if (this.mClientSocket != null)
            {
                if (this.mClientSocket.Connected)
                {
                    this.mClientSocket.Shutdown(SocketShutdown.Both);
                }
                this.mClientSocket.Close();
                this.mClientSocket = null;
            }
        }

        public void Send(byte[] data, int offset, int count)
        {
            try
            {
                int sent = this.mClientSocket.Send(data, offset, count, SocketFlags.None);
                this.HandleRawdata(this.OnSendRawdata, data, offset, sent);
                this.HandleProcess(this.OnSendSuccess);
            }
            catch
            {
                this.HandleFailure(this.OnSendFailure, SocketError.SocketError);
            }
        }

        public void Send(DatagramMessage message)
        {
            byte[] data = this.mDatagramManager.EnpackMessage(message);
            this.HandleMessage(this.OnSendMessage, message);
            this.Send(data, 0, data.Length);
        }

        public void Recv()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mClientSocket.RemoteEndPoint;
            args.SetBuffer(this.mRecvBuffer, 0, this.mRecvBuffer.Length);
            args.UserToken = null;
            args.Completed += this.OnRecvCompleted;
            if (!this.mClientSocket.ReceiveAsync(args))
            {
                this.OnRecvCompleted(this, args);
            }
        }
        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                int count = args.BytesTransferred;
                if(count <= 0)
                {
                    this.HandleFailure(this.OnRecvFailure, SocketError.ConnectionAborted);
                    return;
                }

                this.mRecvMemory.Position = this.mRecvMemory.Length;
                this.mRecvMemory.Write(this.mRecvBuffer, 0, count);
                if (count < this.mRecvBuffer.Length)
                {
                    this.mRecvMemory.Position = this.mMemoryPosition;
                    for (byte[] data = this.mDatagramManager.AnalyzeMessage(this.mRecvMemory);
                        data != null;
                        data = this.mDatagramManager.AnalyzeMessage(this.mRecvMemory))
                    {
                        if (this.OnRecvRawdata != null)
                        {
                            this.OnRecvRawdata(data, 0, data.Length);
                        }
                        if (this.OnRecvMessage != null)
                        {
                            DatagramMessage message = this.mDatagramManager.DepackMessage(data);
                            this.OnRecvMessage(message);
                        }
                    }
                    if (this.mRecvMemory.Position > this.MemorySize && this.mRecvMemory.Position == this.mRecvMemory.Length)
                    {
                        this.mRecvMemory.Close();
                        this.mRecvMemory = new MemoryStream();
                    }
                    this.mMemoryPosition = this.mRecvMemory.Position;
                }
                this.HandleProcess(this.OnRecvSuccess);
            }
            else
            {
                this.HandleFailure(this.OnRecvFailure, args.SocketError);
            }
        }
        #endregion

        #region Private Methods
        private void HandleProcess(ProcessHandler handler)
        {
            if (handler != null)
            {
                handler();
            }
        }

        private void HandleFailure(FailureHandler handler, SocketError error)
        {
            if (handler != null)
            {
                handler(error);
            }
        }

        private void HandleRawdata(RawdataHandler handler, byte[] data, int offset, int count)
        {
            if (handler != null)
            {
                handler(data, offset, count);
            }
        }

        private void HandleMessage(MessageHandler handler, DatagramMessage message)
        {
            if (handler != null)
            {
                handler(message);
            }
        }
        #endregion
    }


    public class AsyncNetworkClient
    {
        public int AsyncTimeout = 1000; // ms
        public int SendTimeout = 0;
        public int RecvTimeout = 0;
        public int BufferSize = 8192;
        public int MemorySize = 8192;

        public delegate void ProcessHandler();
        public delegate void FailureHandler(SocketError error);
        public delegate void RawdataHandler(byte[] data, int offset, int count);
        public delegate void MessageHandler(DatagramMessage message);
        public ProcessHandler OnConnSuccess;
        public FailureHandler OnConnFailure;
        public ProcessHandler OnSendSuccess;
        public FailureHandler OnSendFailure;
        public RawdataHandler OnSendRawdata;
        public MessageHandler OnSendMessage;
        public ProcessHandler OnRecvSuccess;
        public FailureHandler OnRecvFailure;
        public RawdataHandler OnRecvRawdata;
        public MessageHandler OnRecvMessage;

        private static ManualResetEvent msClientDone = new ManualResetEvent(false);

        Socket mClientSocket;
        IPEndPoint mLocalEndPoint;
        IPEndPoint mRemoteEndPoint;
        byte[] mRecvBuffer;
        MemoryStream mRecvMemory;
        long mMemoryPosition;
        DatagramManager mDatagramManager;

        #region Constructor
        public AsyncNetworkClient()
        {
            this.mClientSocket = null;
            this.mRemoteEndPoint = null;
            this.mRecvBuffer = new byte[this.BufferSize];
            this.mRecvMemory = new MemoryStream();
            this.mMemoryPosition = 0;
            this.mDatagramManager = new DatagramManager();
        }
        #endregion

        #region Public Methods
        public void RegisterMessage<T>(ushort id) where T : DatagramMessage
        {
            this.mDatagramManager.RegisterMessage(typeof(T), id);
        }

        public void ClearMessage()
        {
            this.mDatagramManager.ClearMessages();
        }

        public void Connect(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            this.mLocalEndPoint = localEndPoint;
            this.mRemoteEndPoint = remoteEndPoint;
            this.Connect();
        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            this.mLocalEndPoint = null;
            this.mRemoteEndPoint = remoteEndPoint;
            this.Connect();
        }

        public void Connect(string localAddr, int localPort, string remoteAddr, int remotePort)
        {
            this.mLocalEndPoint = new IPEndPoint(IPAddress.Parse(localAddr), localPort);
            this.mRemoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort);
            this.Connect();
        }

        public void Connect(string remoteAddr, int remotePort)
        {
            this.mLocalEndPoint = null;
            this.mRemoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort);
            this.Connect();
        }

        public void Connect()
        {
            if (this.mRemoteEndPoint == null)
                throw new NullReferenceException("RemoteEndPoint is null");

            this.Disconnect();

            this.mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (this.RecvTimeout > 0)
                this.mClientSocket.ReceiveTimeout = this.RecvTimeout;
            if (this.SendTimeout > 0)
                this.mClientSocket.SendTimeout = this.SendTimeout;
            if (this.mLocalEndPoint != null)
                this.mClientSocket.Bind(this.mLocalEndPoint);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mRemoteEndPoint;
            args.UserToken = null;
            args.Completed += this.OnConnectCompleted;

            msClientDone.Reset();
            if (!this.mClientSocket.ConnectAsync(args))
            {
                this.OnConnectCompleted(this, args);
            }
            msClientDone.WaitOne(this.AsyncTimeout);
        }
        private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                this.HandleProcess(this.OnConnSuccess);
            }
            else
            {
                this.HandleFailure(this.OnConnFailure, args.SocketError);
            }
            msClientDone.Set();
        }

        public void Disconnect()
        {
            if (this.mClientSocket != null)
            {
                if (this.mClientSocket.Connected)
                {
                    this.mClientSocket.Shutdown(SocketShutdown.Both);
                }
                this.mClientSocket.Close();
                this.mClientSocket = null;
            }
        }

        public void Send(byte[] data, int offset, int count)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mClientSocket.RemoteEndPoint;
            args.SetBuffer(data, offset, count);
            args.UserToken = null;
            args.Completed += this.OnSendCompleted;

            msClientDone.Reset();
            if (!this.mClientSocket.SendAsync(args))
            {
                this.OnSendCompleted(this, args);
            }
            msClientDone.WaitOne(this.AsyncTimeout);
        }
        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                this.HandleRawdata(this.OnSendRawdata, args.Buffer, args.Offset, args.Count);
                this.HandleProcess(this.OnSendSuccess);
            }
            else
            {
                this.HandleFailure(this.OnSendFailure, args.SocketError);
            }
            msClientDone.Set();
        }

        public void Send(DatagramMessage message)
        {
            byte[] data = this.mDatagramManager.EnpackMessage(message);
            this.HandleMessage(this.OnSendMessage, message);
            this.Send(data, 0, data.Length);
        }

        public void Recv()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mClientSocket.RemoteEndPoint;
            args.SetBuffer(this.mRecvBuffer, 0, this.mRecvBuffer.Length);
            args.UserToken = null;
            args.Completed += this.OnRecvCompleted;
            if (!this.mClientSocket.ReceiveAsync(args))
            {
                this.OnRecvCompleted(this, args);
            }
        }
        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                int count = args.BytesTransferred;
                if (count <= 0)
                {
                    this.HandleFailure(this.OnRecvFailure, SocketError.ConnectionAborted);
                    return;
                }

                this.mRecvMemory.Position = this.mRecvMemory.Length;
                this.mRecvMemory.Write(this.mRecvBuffer, 0, count);
                if (count < this.mRecvBuffer.Length)
                {
                    this.mRecvMemory.Position = this.mMemoryPosition;
                    for (byte[] data = this.mDatagramManager.AnalyzeMessage(this.mRecvMemory);
                        data != null;
                        data = this.mDatagramManager.AnalyzeMessage(this.mRecvMemory))
                    {
                        if (this.OnRecvRawdata != null)
                        {
                            this.OnRecvRawdata(data, 0, data.Length);
                        }
                        if (this.OnRecvMessage != null)
                        {
                            DatagramMessage message = this.mDatagramManager.DepackMessage(data);
                            this.OnRecvMessage(message);
                        }
                    }
                    if (this.mRecvMemory.Position > this.MemorySize && this.mRecvMemory.Position == this.mRecvMemory.Length)
                    {
                        this.mRecvMemory.Close();
                        this.mRecvMemory = new MemoryStream();
                    }
                    this.mMemoryPosition = this.mRecvMemory.Position;
                }
                this.HandleProcess(this.OnRecvSuccess);
            }
            else
            {
                this.HandleFailure(this.OnRecvFailure, args.SocketError);
            }
        }
        #endregion

        #region Private Methods
        private void HandleProcess(ProcessHandler handler)
        {
            if (handler != null)
            {
                handler();
            }
        }

        private void HandleFailure(FailureHandler handler, SocketError error)
        {
            if (handler != null)
            {
                handler(error);
            }
        }

        private void HandleRawdata(RawdataHandler handler, byte[] data, int offset, int count)
        {
            if (handler != null)
            {
                handler(data, offset, count);
            }
        }

        private void HandleMessage(MessageHandler handler, DatagramMessage message)
        {
            if (handler != null)
            {
                handler(message);
            }
        }
        #endregion
    }
}
