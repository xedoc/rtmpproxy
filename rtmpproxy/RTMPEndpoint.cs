using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using rtmpproxy.Messages;
using System.IO;

namespace rtmpproxy
{
    public enum RTMPState
    {
        Unitialized,    //Just connected 
        VersionSent,    //0x03 or other version number is sent, waiting for a payload
        VersionReceived,
        AckSent,        //Waiting for a copy of payload
        AckReceived,        
        HandshakeDone,   //Start the rock'n roll
        Unknown
    }
    class RTMPEndpoint
    {

        private const int versionPayloadLength = 1536;
        private const int versionNumber = 3;
        private readonly static byte[] emptyArray = new byte[0];
        private byte[] currentData;
        private RTMPPacket previousPacket;

        private RTMPEndpointOptions options;
        #region Events
        public EventHandler<AMFCallData> OnReleaseStream;
        public EventHandler<AMFCallData> OnPublish;
        public EventHandler<AMFCallData> OnFCPublish;
        public EventHandler<AMFCallData> OnCreateStream;
        public EventHandler<ConnectData> OnConnect;
        #endregion

        public RTMPState CurrentState
        {
            get;
            set;
        }

        public TCPSocket Socket
        {
            get;
            set;
        }
        public RTMPEndpoint(TCPSocket socket)
        {
            previousPacket = null;
            Socket = socket;
            Version = versionNumber;
            currentData = emptyArray;
            Socket.OnDataReceived += OnData;
            CurrentState = RTMPState.Unitialized;

            options = new RTMPEndpointOptions();            
        }
        public byte Version
        {
            get;
            set;
        }
        private void OnData( object sender, SocketData socketData )
        {
            var data = socketData.Data;
            currentData = ArrayUtil.ConcatArrays(currentData, data);

            var parsedBytesCount = 1;
            while ( parsedBytesCount > 0)
            {
                parsedBytesCount = ParseReceivedData();
                currentData = ArrayUtil.Right(currentData, parsedBytesCount);
            }
        }
        private int ParseReceivedData()
        {
            if (currentData.Length <= 0)
                return 0;

            RTMPPacket packet = new RTMPPacket(PacketType.Unknown);

            switch (CurrentState)
            {

                case RTMPState.Unitialized:
                    {
                        packet = new RTMPPacket(PacketType.VersionNumber);
                        if (packet.InitWith(currentData))
                            CurrentState = RTMPState.VersionReceived;
                    }
                    break;
                case RTMPState.VersionReceived:
                    {
                        packet = new RTMPPacket(PacketType.Handshake);
                        if (packet.InitWith(currentData))
                        {
                            var sendData = ArrayUtil.ConcatArrays(new byte[1]{versionNumber},packet.RawData);
                            sendData = ArrayUtil.ConcatArrays(sendData,packet.RawData);
                            Socket.Send(sendData);
                            CurrentState = RTMPState.VersionSent;
                        }
                    }
                    break;
                case RTMPState.VersionSent:
                    packet = new RTMPPacket(PacketType.Handshake);
                    if (packet.InitWith(currentData))
                    {
                        CurrentState = RTMPState.HandshakeDone;
                    }
                    break;
                case RTMPState.HandshakeDone:
                    {
                        if (previousPacket == null)
                        {
                            previousPacket = new RTMPPacket(PacketType.Chunk);
                            if( !previousPacket.InitWith(currentData) )
                                return 0;
                            packet = previousPacket;
                        }
                        else
                        {
                            packet = new RTMPPacket(PacketType.Chunk);
                            if (!packet.InitWith(currentData, previousPacket))
                                return 0;
                            else
                                previousPacket = packet;

                        }
                        if (packet.MessageLength > 0 && packet.MessageLength < packet.MessageData.Length)
                            return 0;

                        Debug.Print(String.Format("Received RTMP chunk {0} bytes, Msg Id: {1} : {2}", packet.RawData.Length, packet.MessageTypeId, BitConverter.ToString(packet.RawData)));
                        Debug.Print(String.Format("Chunk payload: {0}", BitConverter.ToString(packet.MessageData)));

                        var msgID = packet.MessageTypeId;
                        if (!ParseMessage((MessageID)msgID, packet.MessageData))
                                Debug.Print("Failed to parse control message");
                    }
                    break;
                default:
                   
                    break;
            }
            return packet.RawLength;
        }
        private void Send(byte[] data)
        {
            Socket.Send(data);
        }
        private bool ParseMessage( MessageID id, byte[] payload )
        {
            switch (id)
            {
                //Parse control messages 1-7
                case MessageID.SetChunkSize:
                    options.ChunkSize = ArrayUtil.BigIndianInt(payload, 0, 4);
                    break;
                case MessageID.WindowAcknowledge:
                    options.WindowSize = ArrayUtil.BigIndianInt(payload, 0, 4);
                    break;
                
                // Parse commands like connect
                case MessageID.CommandAMF0:
                    var command = ArrayUtil.AMF0String(payload, 0);
                    switch (command.ToLower())
                    {
                        case "connect":
                            var cmdConnect = new CmdConnect( payload );
                            if (OnConnect != null)
                                OnConnect(this, new ConnectData(cmdConnect));
                            break;
                        case "releasestream":
                            if (OnReleaseStream != null)
                                OnReleaseStream(this, new AMFCallData(payload) );
                            break;
                        case "fcpublish":
                            if (OnFCPublish != null)
                                OnFCPublish(this, new AMFCallData(payload) );
                            break;
                        case "createstream":
                            if (OnCreateStream != null)
                                OnCreateStream(this, new AMFCallData(payload));
                            break;
                        case "publish":
                            if (OnPublish != null)
                                OnPublish(this, new AMFCallData(payload));
                            break;
                        default:
                            return false;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
        public void ReplyConnect()
        {
            var dataChunks = new byte[][] {
                new WinAck(options.WindowSize).Serialize(),
                new Bandwidth(options.WindowSize).Serialize(),
                new StreamBegin().Serialize(),
                new SetChunkSize().Serialize(),
                new ResultSuccess().Serialize()
            };

            var sendData = new MemoryStream();
            sendData.Position = 0;
            foreach (var chunk in dataChunks)
                sendData.Write(chunk, 0, chunk.Length);

            Send(sendData.ToArray());
        }
        public void ReplyCreateStream()
        {
            double param0 = 4;
            object param1 = null;
            double param2 = 1;
            var properties = new List<AMFProperty>();

            properties.Add(new AMFProperty("", param0, AMF0Types.Number));
            properties.Add(new AMFProperty("", param1, AMF0Types.Null));
            properties.Add(new AMFProperty("", param2, AMF0Types.Number));

            var reply = new AMFCall("_result", properties);

            Send(reply.Serialize());
        }

        public void ReplyPublish()
        {
            var sendData = new MemoryStream();
            var streamBegin = new StreamBegin() { ID = 1 }.Serialize();

            double param1 = 0;
            object param2 = null;

            AMFObject param3 = new AMFObject();

            param3.SetProperty("level", "status", AMF0Types.String);
            param3.SetProperty("code", "NetStream.Publish.Start", AMF0Types.String);
            param3.SetProperty("description", "jtv_ASDF", AMF0Types.String);
            param3.SetProperty("client", (double)1234, AMF0Types.Number);

            var properties = new List<AMFProperty>();

            properties.Add(new AMFProperty("", param1, AMF0Types.Number));
            properties.Add(new AMFProperty("", param2, AMF0Types.Null));

            var reply = new AMFCall("onStatus", properties);

            sendData.Write(streamBegin, 0, streamBegin.Length);
            
            var resultSer = reply.Serialize();
            sendData.Write(resultSer, 0, resultSer.Length);
            
            resultSer = param3.Serialize();
            sendData.Write(resultSer, 0, resultSer.Length);

            Send(sendData.ToArray());
        }
    }
}
