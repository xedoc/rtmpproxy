using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rtmpproxy.Messages
{
    class Header
    {

        public byte Format
        {
            get;
            set;
        }
        public byte ChunkStreamID
        {
            get;
            set;
        }
        public UInt32 Timestamp
        {
            get;
            set;
        }
        public byte TypeID
        {
            get;
            set;
        }
        public UInt32 StreamID
        {
            get;
            set;
        }
        public UInt32 BodySize
        {
            get;
            set;
        }

        public byte[] Serialize()
        {
            var timeStamp = ArrayUtil.Right(BitConverter.GetBytes(Timestamp).Reverse().ToArray(), 1);
            var bodySize = ArrayUtil.Right(BitConverter.GetBytes(BodySize).Reverse().ToArray(), 1);
            var streamID = BitConverter.GetBytes(StreamID).Reverse().ToArray();
            MemoryStream result = new MemoryStream();

            result.Position = 0;
            result.WriteByte( (byte)(( Format & 0xC0 ) + ( ChunkStreamID & 0x3F )) );            
            result.Write(timeStamp,0,timeStamp.Length);
            result.Write(bodySize, 0, bodySize.Length);
            result.WriteByte(TypeID);
            result.Write(streamID,0,streamID.Length);
            
            return result.ToArray();
        }
    }
}
