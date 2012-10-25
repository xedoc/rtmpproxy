using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace rtmpproxy.Messages
{
    class SetChunkSize
    {
        private UInt32 chunkSize;
        public SetChunkSize()
        {
            Size = 512;
        }
        public SetChunkSize(byte[] payload)
        {
            Size = ArrayUtil.BigIndianInt(payload, 0, 4);
        }
        public UInt32 Size
        {
            get {return chunkSize;}
            set {chunkSize = value;}
        }
        public byte[] Serialize()
        {
            var result = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00, 0x00 };
            result = ArrayUtil.ConcatArrays(result, BitConverter.GetBytes(Size).Reverse().ToArray());
            return result;
        }
    }
}
