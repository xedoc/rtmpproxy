using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy
{
    class RTMPEndpointOptions
    {
        private UInt32 windowSize = 2500000;
        public RTMPEndpointOptions()
        {
            ChunkSize = 0;
        }
        public UInt32 ChunkSize
        {
            get;
            set;
        }
        public UInt32 WindowSize
        {
            get { return windowSize; }
            set { windowSize = value; }
        }
    }
}
