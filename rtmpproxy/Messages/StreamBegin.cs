﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy.Messages
{
    class StreamBegin
    {
        public StreamBegin()
        {
            ID = 0;
        }
        public byte ID
        {
            get;
            set;
        }
        public byte[] Serialize()
        {
            return new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, ID };
        }
    }
}
