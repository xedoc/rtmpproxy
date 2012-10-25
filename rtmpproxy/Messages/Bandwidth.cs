using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy.Messages
{
    class Bandwidth
    {
        public Bandwidth(UInt32 bw)
        {
            Size = bw;
        }
        public UInt32 Size
        {
            get;
            set;
        }

        public byte[] Serialize()
        {
            //Header
            var result = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00 };            
            //Bandwidth value
            result = ArrayUtil.ConcatArrays(result, BitConverter.GetBytes(Size).Reverse().ToArray());            
            //Dynamic 
            result = ArrayUtil.ConcatArrays(result, new byte[] { 0x02 });
            return result;
        }
    }
}
