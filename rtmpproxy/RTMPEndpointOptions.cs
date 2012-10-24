using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy
{
    class RTMPEndpointOptions
    {
        public RTMPEndpointOptions()
        {
            ChunkSize = 0;
            WindowSize = 0;

        }
        public UInt32 ChunkSize
        {
            get;
            set;
        }
        public UInt32 WindowSize
        {
            get;
            set;
        }
    }
}
