using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace rtmpproxy
{
    class TCPServer
    {
        private const int defaultPort = 1935;
        private TcpListener server;
        private List<TCPSocket> sockets;
        private int port;
        private IPAddress ipaddress;

        public EventHandler<SocketData> m_ClientConnect;
        public event EventHandler<SocketData> OnClientConnect
        {
            add { m_ClientConnect += value; }
            remove { m_ClientConnect -= value; }
        }
        public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        public TCPServer(String ipAddress, int Port)
        {
            sockets = new List<TCPSocket>();
            IPAddress.TryParse(ipAddress, out ipaddress);
            port = Port;
        }
        public bool Start()
        {
            try
            {
                server = new TcpListener(new IPEndPoint(ipaddress, defaultPort));
                server.Start();
                ThreadPool.QueueUserWorkItem(arg => CheckPendingClients());
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void CheckPendingClients()
        {
            while (true)
            {
                if (server.Pending())
                {
                    tcpClientConnected.Reset();
                    BeginAcceptClient();
                    tcpClientConnected.WaitOne();
                }
                Thread.Sleep(10);
            }

        }
        private void BeginAcceptClient()
        {
            server.BeginAcceptTcpClient( new AsyncCallback(AcceptClient), server);
        }
        private void AcceptClient( IAsyncResult asRes)
        {
            var tcpclient = ((TcpListener)asRes.AsyncState).EndAcceptTcpClient(asRes);
            tcpclient.ReceiveBufferSize = 1024 * 1024;
            tcpclient.SendBufferSize = 1024 * 1024;

            var socket = new TCPSocket(tcpclient.Client);            
            socket.OnDisconnect += OnClientDisconnect;

            sockets.Add(socket);
            if (m_ClientConnect != null)
                m_ClientConnect(this, new SocketData(socket));

            Debug.Print("Client connected. IP:{0}", socket.IP);
            tcpClientConnected.Set();
        }
        private void OnClientDisconnect(object sender, EventArgs e)
        {
            tcpClientConnected.WaitOne();
            if (sockets.Count > 0)
            {
                sockets.Remove((TCPSocket)sender);
                Debug.Print("Client disconnected");
            }

        }
    }
}
