using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace rtmpproxy
{
    class TCPSocket
    {
        private const int rcvBufferSize = 4096;
        private Socket _socket;
        private static ManualResetEvent receiveFlag = new ManualResetEvent(false);
        private static ManualResetEvent sendFlag = new ManualResetEvent(true);
        private List<byte[]> sendBuffers;
        
        private byte[] rcvBuffer;
        private bool _connected;
        public EventHandler<SocketData> m_DataReceived;
        public event EventHandler<SocketData> OnDataReceived
        {
            add { m_DataReceived += value; }
            remove { m_DataReceived -= value; }
        }

        public EventHandler m_Disconnect;
        public event EventHandler OnDisconnect
        {
            add { m_Disconnect += value; }
            remove { m_Disconnect -= value; }
        }
        public TCPSocket(Socket socket)
        {
            if (socket == null)
                throw new Exception("Socket shouldn't be null!");

            rcvBuffer = new byte[rcvBufferSize];
            sendBuffers = new List<byte[]>();
            _socket = socket;
            Connected = true;
            ThreadPool.QueueUserWorkItem(arg => CheckPendingData());
            ThreadPool.QueueUserWorkItem(arg => SendPendingData());

        }
        private bool Connected
        {
            get {return _connected; }
            set {
                    _connected = value;
                    if (value == false)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket.Close(1);
                        if (m_Disconnect != null)
                            m_Disconnect(this, EventArgs.Empty);

                    }
                }
        }
        private void ReceiveData(IAsyncResult result)
        {
            int bytesRead;
            try
            {
                bytesRead = _socket.EndReceive(result);
                if (bytesRead > 0)
                {

                    var receivedBytes = new byte[bytesRead];
                    Buffer.BlockCopy(rcvBuffer, 0, receivedBytes, 0, bytesRead);
                    Debug.Print(String.Format("Received {0} bytes: {1}", bytesRead, BitConverter.ToString(receivedBytes)));
                    if (m_DataReceived != null)
                    {
                        m_DataReceived(this, new SocketData(this, receivedBytes));
                    }
                }
                else
                {
                    Connected = false;
                }
            }
            catch (Exception e){ 
                Debug.Print( String.Format("Receive error: {0}",e.Message ));
            }
            finally
            {
                receiveFlag.Set();
            }
            

        }
        public string IP
        {
            get { return _socket.RemoteEndPoint.ToString(); }
        }
        public void Send(byte[] data)
        {
            sendBuffers.Add(data);
            sendFlag.Set();
        }

        private void SendPendingData()
        {
                while ( Connected )
                {
                    sendFlag.WaitOne();
                    while (sendBuffers.Count > 0)
                    {
                        if (sendBuffers[0] != null && sendBuffers[0].Length > 0)
                        {
                            _socket.Send(sendBuffers[0]);
                            Debug.Print(String.Format("Sent {0} bytes: {1}", sendBuffers[0].Length, BitConverter.ToString(sendBuffers[0])));
                            sendBuffers.RemoveAt(0);
                        }
                    }
                    sendFlag.Reset();
                }
        }
        private void CheckPendingData()
        {
            while (Connected)
            {
                receiveFlag.Reset();
                _socket.BeginReceive(
                                rcvBuffer, 0,
                                rcvBuffer.Length,
                                SocketFlags.None,
                                new AsyncCallback(ReceiveData),
                                this);
                receiveFlag.WaitOne();
            }
              
        }

    }
}
