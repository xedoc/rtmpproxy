using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace rtmpproxy
{
    class RTMPServer
    {
        private TCPServer tcpserver;
        private int _port = 1935;
        private string _address = "0.0.0.0";
        private List<RTMPEndpoint> clients;

        public RTMPServer(string ListenAt, int Port)
        {
            if (String.IsNullOrEmpty(ListenAt) || Port <= 0)
            {
                _port = Port;
                _address = ListenAt;
            }
            else
            {
                throw new Exception("Invalid address or port");
            }
        }
        public RTMPServer()
        {        
    
        }
        private void OnConnect(object sender, ConnectData data)
        {
            var endpoint = sender as RTMPEndpoint;
            endpoint.ReplyConnect();
        }
        private void OnNewClient(object sender, SocketData socketdata)
        {
            var client = new RTMPEndpoint(socketdata.Socket);
            client.OnConnect += OnConnect;
            clients.Add(client);
        }
        public bool Start()
        {
            tcpserver = new TCPServer(_address, _port);
            clients = new List<RTMPEndpoint>();
            if (!tcpserver.Start())
            {
                Debug.Print("TCP server start failed");
                return false;
            }
            else
            {
                tcpserver.OnClientConnect += OnNewClient;
            }
            return true;
        }
    }
}
