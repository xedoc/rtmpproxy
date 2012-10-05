﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
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
                            previousPacket.InitWith(currentData);
                            packet = previousPacket;
                        }
                        else
                        {
                            packet = new RTMPPacket(PacketType.Chunk);
                            if (packet.InitWith(currentData, previousPacket))
                            {
                                previousPacket = packet;
                            }

                        }
                        Debug.Print(String.Format("Received RTMP chunk {0} bytes: {1}", packet.RawData.Length, BitConverter.ToString(packet.RawData)));

                    }
                    break;
                default:
                   
                    break;
            }
            return packet.RawLength;
        }

    }
}