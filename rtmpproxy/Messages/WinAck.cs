using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy.Messages
{
    class WinAck
    {
        public WinAck(UInt32 size)
        {
            Size = size;
        }
        public UInt32 Size
        {
            get;
            set;
        }
        public byte[] Serialize()
        {
            var result = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x05, 0x00, 0x00, 0x00, 0x00 };
            result = ArrayUtil.ConcatArrays(result, BitConverter.GetBytes(Size).Reverse().ToArray());
            return result;
        }
    }
}
