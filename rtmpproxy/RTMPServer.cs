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
        private void OnCreateStream( object sender, AMFCallData data )
        {
            var endpoint = sender as RTMPEndpoint;
            endpoint.ReplyCreateStream();
        }
        private void OnReleaseStream( object sender, AMFCallData data )
        {
        }
        private void OnFCPublish(object sender, AMFCallData data)
        {
        }
        private void OnPublish(object sender, AMFCallData data)
        {
            var endpoint = sender as RTMPEndpoint;
            endpoint.ReplyPublish();
        }
        private void OnNewClient(object sender, SocketData socketdata)
        {
            var client = new RTMPEndpoint(socketdata.Socket);
            client.OnConnect += OnConnect;
            client.OnCreateStream += OnCreateStream;
            client.OnReleaseStream += OnReleaseStream;
            client.OnFCPublish += OnFCPublish;
            client.OnPublish += OnPublish;

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
