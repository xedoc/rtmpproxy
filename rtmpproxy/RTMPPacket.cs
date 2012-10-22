using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy
{

    //
    // + RTMP Packet structure +
    //+-------------+----------------+-------------------+--------------+ 
    //| basic header|chunk msg header|extended time stamp|   chunk data | 
    //+-------------+----------------+-------------------+--------------+ 
    // Basic header have variable length 1-3 bytes
    // Extended time stamp and chunk msg header is optional
    class RTMPPacket
    {
        private const int defaultVersion = 3;
        private const int handshakeLength = 1536;
        private readonly static byte[] emptyArray = new byte[0];
        public RTMPPacket(PacketType packetType)
        {
            Type = packetType;
            RawData = emptyArray;
            RawLength = 0;
        }
        public PacketType Type
        {
            get;
            set;
        }
        public byte[] RawData
        {
            get;
            set;
        }
        public int RawLength
        {
            get;
            set;
        }
        public ChunkType ChunkType
        {
            get;
            set;
        }
        //Chunk basic header: 1 to 3 bytes 
        //  This field encodes the chunk stream ID and the chunk type. Chunk type determines the format of the encoded message header. 
        //  The length depends entirely on the chunk stream ID, which is a variable-length field. 
        //  First byte:
        //      bits 7-6 - Chunk Type
        //      bits 5-0 - if >= 2 - Stream ID. 
        //      Else if == 0, 2 bytes long version. Stream ID = 64 + second byte. 
        //      Else if == 1, 3 bytes long version. Stream ID = 64 + second byte + (third byte * 256)
        private byte[] RawBasicHeader
        {
            get;
            set;
        }
        //Chunk message header: 0, 3, 7, or 11 bytes 
        //  This field encodes information about the message being sent (whether in whole or in part). 
        //  The length can be determined using the chunk type specified in the basic header. 

        //  If Chunk Type == 0, Length of this header = 11 bytes
        //  Bytes 1-3 - Absolute Timestamp. If its >= 0x00ffffff then extended timestamp must be present otherwise this is entire timestamp
        //  Bytes 4-6 - Message Length
        //  Byte 7 - Message Type Id
        //  Byte 8-11 - Message Stream Id
        
        //  If Chunk Type == 1, length = 7 bytes
        //  Bytes 1-3 - Timestamp Delta. If its >= 0x00ffffff then extended timestamp must be present otherwise this is entire timestamp
        //  Bytes 4-6 - Message Length
        //  Byte 7 - Message Type Id

        //  If Chunk Type == 2, length = 3 bytes
        //  Bytes 1-3 - Timestamp Delta.
        
        //  If Chunk Type == 3, length = 0 bytes
        //  All properties should be taken from the previous chunk
        private byte[] RawMessageHeader
        {
            get;
            set;
        }
        //Extended timestamp: 0 or 4 bytes 
        //  This field MUST be sent when the normal timsestamp is => 0xffffff
        private byte[] RawExtendedTimeStamp
        {
            get;
            set;
        }

        public byte[] RawChunkData
        {
            get;
            set;
        }
        public UInt32 Timestamp
        {
            get;
            set;
        }
        public int ExtendedTimeStampLength
        {
            get;
            set;
        }
        public UInt32 ExtendedTimestamp
        {
            get;
            set;
        }
        public int MessageLength
        {
            get;
            set;
        }
        public int MessageHeaderLength
        {
            get;
            set;
        }
        public int BasicHeaderLength
        {
            get;
            set;
        }
        public UInt32 ChunkStreamId
        {
            get;
            set;
        }
        public UInt32 MessageStreamId
        {
            get;
            set;
        }
        public byte MessageTypeId
        {
            get;
            set;
        }
        private void ParseChunkType(byte HeaderByte)
        {
            switch (HeaderByte >> 6) // first 2 bits
            {
                case 0:
                    {
                        ChunkType = rtmpproxy.ChunkType.Header11;
                        MessageHeaderLength = 11;
                    }
                    break;
                case 1:
                    {
                        ChunkType = rtmpproxy.ChunkType.Header7;
                        MessageHeaderLength = 7;
                    }
                    break;
                case 2:
                    {
                        ChunkType = rtmpproxy.ChunkType.Header3;
                        MessageHeaderLength = 3;
                    }
                    break;
                case 3:
                    {
                        ChunkType = rtmpproxy.ChunkType.Header0;
                        MessageHeaderLength = 0;
                    }
                    break;
                default:
                    {
                        ChunkType = rtmpproxy.ChunkType.Undefined;
                        MessageHeaderLength = 0;
                    }
                    break;
            }
            if (ChunkType != rtmpproxy.ChunkType.Header0 && 
                ChunkType != rtmpproxy.ChunkType.Undefined)
            {
                switch (HeaderByte & 0x3F)
                {
                    case 0:
                        {
                            BasicHeaderLength = 2;
                        }
                        break;

                    case 1:
                        {
                            BasicHeaderLength = 3;
                        }
                        break;

                    default:
                        {
                            BasicHeaderLength = 1;
                        }
                        break;
                }
            }
            else
            {
                BasicHeaderLength = 0;
            }
        }
        private void ParseBasicHeader()
        {
            switch (BasicHeaderLength)
            {
                case 1:
                    ChunkStreamId = (uint)RawBasicHeader[0] & 0x3f;
                    break;
                case 2:
                    ChunkStreamId = (uint)RawBasicHeader[1] + 64;
                    break;
                case 3:
                    ChunkStreamId = (uint)RawBasicHeader[1] + 64 + ((uint)RawBasicHeader[2] * 256);
                    break;
            }
        }
        private void ParseMessageHeader()
        {
            switch (ChunkType)
            {
                case rtmpproxy.ChunkType.Header3:
                    {
                        Timestamp = ArrayUtil.BigIndianInt(RawMessageHeader, 0, 3);
                    }
                    break;
                case rtmpproxy.ChunkType.Header7:
                    {
                        Timestamp = ArrayUtil.BigIndianInt(RawMessageHeader, 0, 3);
                        ExtendedTimestamp = 0;
                        MessageLength = (int)ArrayUtil.BigIndianInt(RawMessageHeader, 3, 3);
                        MessageTypeId = RawMessageHeader[6];
                    }
                    break;
                case rtmpproxy.ChunkType.Header11:
                    {
                        Timestamp = ArrayUtil.BigIndianInt(RawMessageHeader, 0, 3);
                        MessageLength = (int)ArrayUtil.BigIndianInt(RawMessageHeader, 3, 3);
                        MessageTypeId = RawMessageHeader[6];
                        MessageStreamId = ArrayUtil.BigIndianInt(RawMessageHeader, 7, 3);
                    }
                    break;
                default:
                    break;
            }
        }
        public bool InitWith( byte[] data, RTMPPacket previousPacket = null)
        {

            if (data == null)
                throw new Exception("Can't create packet with null payload");

            var dataLength = data.Length;
            if (dataLength < 1 )
                return false;

            RawLength = 0;

            switch (Type)
            {
                case PacketType.VersionNumber:
                    {
                        RawData = new byte[1] { data[0] }; //RTMP protocol version. Normally == 3
                        RawLength = 1;
                    }
                    break;
                case PacketType.Handshake:
                    {
                        if (dataLength < handshakeLength)
                            return false;

                        if (dataLength == handshakeLength)
                        {
                            RawData = data;
                        }
                        else
                        {
                            RawData = new byte[handshakeLength];  //First 8 bytes should be timestamp[4] & zero[4] by Adobe's specs. It seems XSplit doesn't care about specs. So do I :D
                            Buffer.BlockCopy(data, 0, RawData, 0, handshakeLength);
                        }
                        RawLength = handshakeLength;
                    }
                    break;
                case PacketType.Chunk:
                    {
                        // Fill message/basic header lengths and Chunk Type
                        ParseChunkType(data[0]);

                        var chunkLength = BasicHeaderLength + MessageHeaderLength;
                        // Incomplete packet ?
                        if (dataLength < chunkLength)
                            return false;

                        if (ChunkType == rtmpproxy.ChunkType.Undefined)
                            throw new Exception("Packet with unknown RTMP header received!");

                        switch( ChunkType )
                        {
                            // If header is empty - init this packet with properties from the previous packet                        
                            case rtmpproxy.ChunkType.Header0:
                            {
                                if (previousPacket == null)
                                    throw new Exception("Can't calculate RTMP chunk length. Previous packet is null");

                                MessageLength = previousPacket.MessageLength;
                                ChunkStreamId = previousPacket.ChunkStreamId;                            
                            }
                            break;
                            default:
                            {
                                RawBasicHeader = ArrayUtil.Mid(data, 0, BasicHeaderLength);
                                RawMessageHeader = ArrayUtil.Mid(data, BasicHeaderLength, MessageHeaderLength);
                                
                                ParseBasicHeader();
                                ParseMessageHeader();
                                
                                if (ExtendedTimestamp >= 0x00ffffff)
                                {
                                    RawExtendedTimeStamp = ArrayUtil.Mid(data, BasicHeaderLength + MessageHeaderLength, 4);
                                    ExtendedTimeStampLength = 4;
                                }
                                else
                                {
                                    ExtendedTimeStampLength = 0;
                                }
                                chunkLength += (ExtendedTimeStampLength + MessageLength);
                                if (chunkLength <= dataLength)
                                {
                                    RawLength = chunkLength;
                                    RawData = new byte[RawLength];
                                    Buffer.BlockCopy(data, 0, RawData, 0, RawLength);

                                    RawChunkData = new byte[MessageLength];
                                    Buffer.BlockCopy(data, RawLength - MessageLength, RawChunkData, 0, MessageLength);
                                }
                            }
                            break;
                        }
                    }
                    break;
            }

            if (RawLength > 0)
                return true;
            else
                return false;
        }
    }

}
