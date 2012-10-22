using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace rtmpproxy.Messages
{
    class ControlSetChunkSize
    {
        public ControlSetChunkSize(byte[] payload)
        {
            Size = ArrayUtil.BigIndianInt(payload, 0, 4);
        }
        public UInt32 Size
        {
            get;
            set;
        }
    }
}
