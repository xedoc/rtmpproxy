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
        private AMFObject connObject;

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

            connObject = new AMFObject(connObjectData);
          

            
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
            get
            {
                return connObject.GetProperty("type") as string;
            }
            set
            {
                connObject.SetProperty("type", value, AMF0Types.String);
            }
        }
        public String App
        {
            get { 
                return connObject.GetProperty("app") as string; 
            }
            set 
            {
                connObject.SetProperty("app", value, AMF0Types.String);
            }
        }
        public String FlashVer
        {
            get { 
                return connObject.GetProperty("flashver") as string; 
            }
            set {
                connObject.SetProperty("flashver", value, AMF0Types.String);
            }
        }
        public String SwfUrl
        {
            get 
            {
                return connObject.GetProperty("swfUrl") as string;
            }
            set
            {
                connObject.SetProperty("swfUrl", value, AMF0Types.String);
            }
        }
        public String TcUrl
        {
            get
            {
                return connObject.GetProperty("tcUrl") as string;
            }

            set
            {
                connObject.SetProperty("tcUrl", value, AMF0Types.String);
            }  
        }
        public Boolean Fpad
        {
            get
            {
                var v = connObject.GetProperty("fpad");
                if (v == null)
                    return false;
                else
                    return (bool)v;
            }
            set
            {
                connObject.SetProperty("fpad", value, AMF0Types.Boolean);
            }
        }
        public String PageUrl
        {
            get
            {
                return connObject.GetProperty("pageUrl") as string;
            }

            set
            {
                connObject.SetProperty("pageUrl", value, AMF0Types.String);
            }  
        }
        public Dictionary<AudioCodec, bool> AudioCodecs
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
        public EncodingVersion Encoding
        {
            get;
            set;
        }



        
    }
}
