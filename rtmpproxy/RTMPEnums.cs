using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy
{
    public enum EventType: uint
    {
        MessageStreamBegin = 0
    }

    public enum PacketType
    {
        VersionNumber,
        Handshake,
        Chunk,
        Unknown
    }
    public enum ChunkType
    {
        Header11,   //11 bytes long and so on
        Header7,
        Header3,
        Header0,
        Undefined
    }
    enum MessageID : int
    {
        //Control messages 1-7
        SetChunkSize = 1,
        Abort = 2,
        Acknowledgement = 3,
        UserControl = 4,
        WindowAcknowledge = 5,
        SetPeerBandwidth = 6,
        SetPeerBandwidthPayload = 7,

        //Normal messages
        CommandAMF0 = 20,
        CommandAMF3 = 17,
        DataAMF0 = 18,
        DataAMF3 = 15,
        SharedObjectAMF0 = 19,
        SharedObjectAMF3 = 16,
        Audio = 8,
        Video = 9,
        Aggregate = 22,

    }
    enum AMF0Types : byte
    {
        Number = 0,
        Boolean = 1,
        String = 2,
        Object = 3,
        MovieClip = 4,
        Null = 5,
        Undefined = 6,
        Reference = 7,
        Array = 8,
        ObjectArrayEnd = 9,
        StrictArray = 10,
        Date = 11,
        LongString = 12,
        Unsupported = 13,
        RecordSet = 14,
        XmlObject = 15,
        TypedObject = 16
    }

    enum AudioCodec : int
    {
        Raw = 0x1,
        ADPCM = 0x2,
        MP3 = 0x4,
        NotUsed1 = 0x8,
        NotUsed2 = 0x10,
        NellyMoser8KHz = 0x20,
        NellyMoser44KHz = 0x40,
        G711A = 0x80,
        G711U = 0x100,
        NellyMoser16KHz = 0x200,
        AAC = 0x400,
        Speex = 0x800,
        All = 0x0FFF
    }

    enum VideoCodec : byte
    {
        Obsolete1 = 0x1,
        Obsolete2 = 0x2,
        FlashVideo = 0x4,
        V1ScrSharing = 0x8,
        VP6 = 0x10,
        VP6Alpha = 0x20,
        HomeBrewV = 0x40,
        H264 = 0x80,
        All = 0x0ff
    }

    enum VideoFunction: byte
    {
        ClientSeek = 0x1
    }

    enum EncodingVersion : byte
    {
        AMF0 = 0x0,
        AMF3 = 0x3
    }
}
