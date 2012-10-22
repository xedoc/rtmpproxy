using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using rtmpproxy.Messages;

namespace rtmpproxy
{
    class SocketData:EventArgs
    {
        public SocketData(TCPSocket socket, byte[] data)
        {            
            Socket = socket;
            Data = data;
        }
        public SocketData(TCPSocket socket)
        {
            Socket = socket;
        }
        public TCPSocket Socket
        {
            get;
            set;
        }
        public byte[] Data
        {
            get;
            set;
        }
    }

    class ConnectData : EventArgs
    {
        public CmdConnect ConnectObject
        {
            get;
            set;
        }
    }
}
