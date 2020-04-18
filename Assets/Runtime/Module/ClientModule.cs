using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace ProjectX
{
    public class ClientModule : AppModule
    {
        public System.Action<ClientModule> onRegisterMessages = null;

        private NetworkClient mClient = new NetworkClient();
        private Queue<DatagramMessage> mSendQueue = new Queue<DatagramMessage>();
        private Queue<DatagramMessage> mRecvQueue = new Queue<DatagramMessage>();

        private Dictionary<System.Type, System.Action<DatagramMessage>> mHandlers = new Dictionary<System.Type, System.Action<DatagramMessage>>();

        #region Life Circle
        public override bool Init()
        {
            // register messages
            XCSharp.InvokeAction(this.onRegisterMessages, this);

            // register recv-callback
            this.mClient.OnConnSuccess += this.OnConnSuccess;
            this.mClient.OnConnFailure += this.OnConnFailure;

            this.mClient.OnSendSuccess += this.OnSendSuccess;
            this.mClient.OnSendFailure += this.OnSendFailure;
            this.mClient.OnSendRawdata += this.OnSendRawdata;
            this.mClient.OnSendMessage += this.OnSendMessage;

            this.mClient.OnRecvSuccess += this.OnRecvSuccess;
            this.mClient.OnRecvFailure += this.OnRecvFailure;
            this.mClient.OnRecvRawdata += this.OnRecvRawdata;
            this.mClient.OnRecvMessage += this.OnRecvMessage;

            return true;
        }

        public override void Quit()
        {
            this.mClient.Disconnect();
            this.mClient.ClearMessages();
        }

        public override void Loop(float elapse)
        {
            while (this.mRecvQueue.Count > 0)
            {
                DatagramMessage message = this.mRecvQueue.Dequeue();
                System.Type type = message.GetType();

                // global handlers
                App.funcs.Invoke(type.Name, this, message);

                // scoped handlers
                System.Action<DatagramMessage> handler = null;
                this.mHandlers.TryGetValue(type, out handler);
                if(handler != null)
                {
                    handler(message);
                }
            }
        }
        #endregion

        #region Public Methods
        public void RegisterMessage<T>(ushort dmid) where T : DatagramMessage
        {
            this.mClient.RegisterMessage<T>(dmid);
        }

        public void AttachHandler(System.Type type, System.Action<DatagramMessage> handler)
        {
            if (handler == null)
                return;
            System.Action<DatagramMessage> h = null;
            this.mHandlers.TryGetValue(type, out h);
            if (h == null)
            {
                this.mHandlers.Add(type, handler);
            }
            else
            {
                h += handler;
            }
        }

        public void DetachHandler(System.Type type, System.Action<DatagramMessage> handler)
        {
            if (handler == null)
                return;
            System.Action<DatagramMessage> h = null;
            this.mHandlers.TryGetValue(type, out h);
            if (h != null)
            {
                h -= handler;
            }
            if (h == null)
            {
                this.mHandlers.Remove(type);
            }
        }

        public void Connect(string addr, int port)
        {
            this.mClient.Connect(addr, port);
        }

        public void Disconnect()
        {
            this.mClient.Disconnect();
        }

        public void Send(DatagramMessage message)
        {
            try
            {
                this.mClient.Send(message);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        #region Private Methods
        private void OnConnSuccess()
        {
            this.mClient.Recv();
        }
        private void OnConnFailure(SocketError error)
        {
            Debug.LogError("OnConnFailure: " + error);
        }

        private void OnSendSuccess()
        { }
        private void OnSendFailure(SocketError error)
        {
            Debug.LogError("OnSendFailure: " + error);
        }
        private void OnSendRawdata(byte[] data, int offset, int count)
        { }
        private void OnSendMessage(DatagramMessage message)
        { }

        private void OnRecvSuccess()
        {
            this.mClient.Recv();
        }
        private void OnRecvFailure(SocketError error)
        {
            if (error == SocketError.Interrupted)
                return;
            Debug.LogError("OnRecvFailure: " + error);
        }
        private void OnRecvRawdata(byte[] data, int offset, int count)
        { }
        private void OnRecvMessage(DatagramMessage message)
        {
            this.mRecvQueue.Enqueue(message);
        }
        #endregion
    }
}
