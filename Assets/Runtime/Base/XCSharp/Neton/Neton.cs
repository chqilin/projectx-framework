using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

#if NET_4_6
namespace ProjectX
{
    public class NetonChannel
    {
        public Action<NetonChannel> onOpen = null;
        public Action<NetonChannel> onClose = null;
        public Action<NetonChannel, byte[], int, int> onRecvRawdata = null;
        public Action<NetonChannel, DatagramMessage> onRecvMessage = null;
        public Action<NetonChannel, byte[], int, int> onSendRawdata = null;
        public Action<NetonChannel, DatagramMessage> onSendMessage = null;

        public string name = "";
        public object data = null;

        public const int bufferSize = 8192;
        public const int memorySize = 8192;

        private TcpClient mClient = null;
        private DatagramManager mDatagramManager = null;
        private MemoryStream mRecvMemory = new MemoryStream();
        private long mMemoryPosition = 0;
        private DateTime mActiveTime = DateTime.Now;

        #region Constructors
        public NetonChannel(TcpClient client, DatagramManager dmanager)
        {
            this.mClient = client;
            this.mDatagramManager = dmanager;
        } 
        #endregion

        #region Properties
        public bool connected
        {
            get 
            {
                if (!this.mClient.Client.Connected)
                    return false;
                int sent = this.mClient.Client.Send(new byte[0] { });
                return sent == 0;
            }
        }
        public DateTime activeTime
        {
            get { return this.mActiveTime; }
        }
        #endregion

        #region Public Methods
        public void Open()
        {
            if (this.onOpen != null)
            {
                this.onOpen(this);
            }
            this.RecvLoop();
        }

        public void Close()
        {
            this.mClient.Close();
            if (this.onClose != null)
            {
                this.onClose(this);
            }
        }

        public void Send(DatagramMessage message)
        {
            byte[] rawdata = this.mDatagramManager.EnpackMessage(message);
            this.Send(rawdata, 0, rawdata.Length);
            if(this.onSendMessage != null)
            {
                this.onSendMessage(this, message);
            }
        }

        public async void Send(byte[] data, int offset, int count)
        {
            NetworkStream stream = this.mClient.GetStream();
            await stream.WriteAsync(data, offset, count);
            if (this.onSendRawdata != null)
            {
                this.onSendRawdata(this, data, offset, count);
            }
        } 
        #endregion

        #region Private Methods
        private async void RecvLoop()
        {
            NetworkStream stream = this.mClient.GetStream();
            byte[] data = new byte[bufferSize];
            while (true)
            {
                try
                {
                    int count = await stream.ReadAsync(data, 0, data.Length);
                    if (count > 0)
                    {
                        this.OnRecv(this, data, 0, count);
                    }
                    else
                    {
                        this.Close();
                        break;
                    }
                }
                catch
                {                  
                    this.Close();
                    break;
                }
            }
        }

        private void OnRecv(NetonChannel channel, byte[] data, int offset, int count)
        {
            this.mRecvMemory.Position = this.mRecvMemory.Length;
            this.mRecvMemory.Write(data, offset, count);
            if (count < data.Length - offset)
            {
                this.mRecvMemory.Position = this.mMemoryPosition;
                for (byte[] rawdata = this.mDatagramManager.AnalyzeMessage(this.mRecvMemory);
                    rawdata != null;
                    rawdata = this.mDatagramManager.AnalyzeMessage(this.mRecvMemory))
                {
                    this.mActiveTime = DateTime.Now;
                    if (this.onRecvRawdata != null)
                    {
                        this.onRecvRawdata(this, rawdata, 0, rawdata.Length);
                    }
                    if (this.onRecvMessage != null)
                    {
                        DatagramMessage message = this.mDatagramManager.DepackMessage(data);
                        this.onRecvMessage(this, message);
                    }
                }

                if (this.mRecvMemory.Position > NetonChannel.memorySize && this.mRecvMemory.Position == this.mRecvMemory.Length)
                {
                    this.mRecvMemory.Close();
                    this.mRecvMemory = new MemoryStream();
                }
                this.mMemoryPosition = this.mRecvMemory.Position;
            }
        }
        #endregion
    }

    public class NetonService
    {
        public Action<NetonService> onStart = null;
        public Action<NetonService> onStop = null;
        public Action<NetonService, NetonChannel> onCreateChannel = null;
        public Action<NetonService, NetonChannel> onDeleteChannel = null;
        public Action<NetonService, NetonChannel, byte[], int, int> onRecvRawdata = null;
        public Action<NetonService, NetonChannel, DatagramMessage> onRecvMessage = null;
        public Action<NetonService, NetonChannel, byte[], int, int> onSendRawdata = null;
        public Action<NetonService, NetonChannel, DatagramMessage> onSendMessage = null;

        private TcpListener mListener = null;
        private bool mIsRunning = false;
        private DatagramManager mDatagramManager = new DatagramManager();
        private List<NetonChannel> mChannels = new List<NetonChannel>();
        
        #region Properties
        public bool isRunning
        {
            get { return this.mIsRunning; }
        }

        public List<NetonChannel> channels
        {
            get { return this.mChannels; }
        }
        #endregion

        #region Public Methods
        public void RegisterMessage<T>(ushort dmid) where T : DatagramMessage
        {
            this.mDatagramManager.RegisterMessage(typeof(T), dmid);
        }

        public void RegisterMessage(Type type, ushort dmid)
        {
            this.mDatagramManager.RegisterMessage(type, dmid);
        }

        public void ClearMessages()
        {
            this.mDatagramManager.ClearMessages();
        }

        public void Start(string addr, int port)
        {
            IPAddress ip = IPAddress.Parse(addr); // Dns.GetHostEntry(addr).AddressList[0];
            this.mListener = new TcpListener(ip, port);           
            this.mListener.Start();
            this.mIsRunning = true;

            if (this.onStart != null)
            {
                this.onStart(this);
            }
            
            this.AcceptLoop();
        }

        public void Stop()
        {
            if (this.onStop != null)
            {
                this.onStop(this);
            }

            this.mIsRunning = false;
            this.mListener.Stop();

            lock (this.mChannels)
            {
                foreach (var channel in this.mChannels)
                {
                    channel.Close();
                }
            }
        }

        public void Send(DatagramMessage message, List<NetonChannel> channels)
        {
            if (channels == null || channels.Count == 0)
                return;
            byte[] rawdata = this.mDatagramManager.EnpackMessage(message);
            foreach (var channel in channels)
            {
                if (!channel.connected)
                    continue;
                if (channel.onSendMessage != null)
                {
                    channel.onSendMessage(channel, message);
                }
                channel.Send(rawdata, 0, rawdata.Length);
            }
        }

        public void Send(byte[] data, int offset, int count, List<NetonChannel> channels)
        {
            if (channels == null || channels.Count == 0)
                return;
            foreach (var channel in channels)
            {
                channel.Send(data, offset, count);
            }
        }

        public void Send(DatagramMessage message, Predicate<NetonChannel> predicate = null)
        {
            byte[] rawdata = this.mDatagramManager.EnpackMessage(message);
            this.Send(rawdata, 0, rawdata.Length, channel => 
            {
                if(predicate == null)
                {
                    if(channel.onSendMessage != null)
                    {
                        channel.onSendMessage(channel, message);
                    }
                    return true;
                }

                if(predicate(channel))
                {
                    if (channel.onSendMessage != null)
                    {
                        channel.onSendMessage(channel, message);
                    }
                    return true;
                }

                return false;
            });
        }

        public void Send(byte[] data, int offset, int count, Predicate<NetonChannel> predicate = null)
        {
            if(predicate == null)
            {
                for (int i = 0; i < this.mChannels.Count; i++)
                {
                    var channel = this.mChannels[i];
                    channel.Send(data, offset, count);
                }
                return;
            }

            for (int i = 0; i < this.mChannels.Count; i++)
            {
                var channel = this.mChannels[i];
                if (predicate(channel))
                {
                    channel.Send(data, offset, count);
                }
            }
        }
        #endregion

        #region Private Methods
        private async void AcceptLoop()
        {
            while (this.mIsRunning)
            {
                try
                {
                    var client = await this.mListener.AcceptTcpClientAsync();
                    this.CreateChannel(client);
                }
                catch
                {
                    if(this.mIsRunning)
                    {
                        this.Stop();
                    }
                    break;
                }
            }
        }

        private void CreateChannel(TcpClient client)
        {
            NetonChannel channel = new NetonChannel(client, this.mDatagramManager);
            channel.onRecvRawdata += this.Channel_OnRecvRawdata;
            channel.onRecvMessage += this.Channel_OnRecvMessage;
            channel.onSendRawdata += this.Channel_OnSendRawdata;
            channel.onSendMessage += this.Channel_OnSendMessage;
            channel.onClose += this.DeleteChannel;
            lock (this.mChannels)
            {
                this.mChannels.Add(channel);
            }
            if (this.onCreateChannel != null)
            {
                this.onCreateChannel(this, channel);
            }
            channel.Open();
        }

        private void DeleteChannel(NetonChannel channel)
        {
            channel.onRecvRawdata -= this.Channel_OnRecvRawdata;
            channel.onRecvMessage -= this.Channel_OnRecvMessage;
            channel.onSendRawdata -= this.Channel_OnSendRawdata;
            channel.onSendMessage -= this.Channel_OnSendMessage;
            channel.onClose -= this.DeleteChannel;
            lock (this.mChannels)
            {
                this.mChannels.Remove(channel);
            }
            if (this.onDeleteChannel != null)
            {
                this.onDeleteChannel(this, channel);
            }
        }

        private void Channel_OnRecvRawdata(NetonChannel channel, byte[] data, int offset, int count)
        {
            if (this.onRecvRawdata != null)
            {
                this.onRecvRawdata(this, channel, data, offset, count);
            }
        }

        private void Channel_OnRecvMessage(NetonChannel channel, DatagramMessage message)
        {
            if (this.onRecvMessage != null)
            {
                this.onRecvMessage(this, channel, message);
            }
        }

        private void Channel_OnSendRawdata(NetonChannel channel, byte[] data, int offset, int count)
        {
            if (this.onSendRawdata != null)
            {
                this.onSendRawdata(this, channel, data, offset, count);
            }
        }

        private void Channel_OnSendMessage(NetonChannel channel, DatagramMessage message)
        {
            if (this.onSendMessage != null)
            {
                this.onSendMessage(this, channel, message);
            }
        }
        #endregion
    }
}
#endif