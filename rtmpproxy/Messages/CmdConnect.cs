using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy.Messages
{
    class CmdConnect
    {
        private byte[] objectEnd = {00,00,09};
        private const int posID = 11;
        private const int posConnObj = 10;

        public CmdConnect(byte[] payload)
        {
            int payloadLen = payload.Length;

            double result = 0;
            if (!ArrayUtil.AMF0Number(payload, posID, ref result))
                return;

            TransactionID = result;

            var posConnObjEnd = ArrayUtil.FindPattern(payload, objectEnd, 20);
            if (posConnObjEnd < 0)
                return;
            
            var connObjectData = ArrayUtil.Mid(payload, 20, posConnObjEnd - 20);

            var connObject = new AMFObject(connObjectData);

            App = (string)connObject.GetProperty("app");
            TcUrl = (string)connObject.GetProperty("tcUrl");
            SwfUrl = (string)connObject.GetProperty("swfUrl");
            PageUrl = (string)connObject.GetProperty("pageUrl");
            Type = (string)connObject.GetProperty("type");
            FlashVer = (string)connObject.GetProperty("flashver");
            Fpad = (bool)connObject.GetProperty("fpad");
            
            AudioCodecs = new Dictionary<AudioCodec, bool>();
            VideoCodecs = new Dictionary<VideoCodec, bool>();
            var audioCodecs = new AudioCodec[] {
                    AudioCodec.Raw,
                    AudioCodec.ADPCM,
                    AudioCodec.MP3,
                    AudioCodec.NotUsed1,
                    AudioCodec.NotUsed2,
                    AudioCodec.NellyMoser8KHz,
                    AudioCodec.NellyMoser44KHz,
                    AudioCodec.G711A,
                    AudioCodec.G711U,
                    AudioCodec.NellyMoser16KHz,
                    AudioCodec.AAC,
                    AudioCodec.Speex,
                    AudioCodec.All
                };
            var videoCodecs = new VideoCodec[] {
                    VideoCodec.Obsolete1,
                    VideoCodec.Obsolete2,
                    VideoCodec.FlashVideo,
                    VideoCodec.V1ScrSharing,
                    VideoCodec.VP6,
                    VideoCodec.VP6Alpha,
                    VideoCodec.HomeBrewV,
                    VideoCodec.H264,
                    VideoCodec.All
            };

            foreach( var codec in audioCodecs )
                AudioCodecs.Add( codec, false );

            foreach (var codec in videoCodecs)
                VideoCodecs.Add( codec, false);
        }

        public double TransactionID
        {
            get;
            set;
        }
        public String Type
        {
            get;
            set;
        }
        public String App
        {
            get;
            set;
        }
        public String FlashVer
        {
            get;
            set;
        }
        public String SwfUrl
        {
            get;
            set;
        }
        public String TcUrl
        {
            get;
            set;
        }
        public Boolean Fpad
        {
            get;
            set;
        }
        public Dictionary<AudioCodec,bool> AudioCodecs
        {
            get;
            set;
        }
        public Dictionary<VideoCodec,bool> VideoCodecs
        {
            get;
            set;
        }
        public VideoFunction VideoFunction
        {
            get;
            set;
        }
        public String PageUrl
        {
            get;
            set;
        }
        public EncodingVersion Encoding
        {
            get;
            set;
        }



        
    }
}
