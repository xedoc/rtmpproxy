using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy.Messages
{
    class CmdConnect
    {
        public CmdConnect(byte[] payload)
        {
            var payloadLen = payload.Length;
            if( payloadLen <= 3 )
                return;
            if (ArrayUtil.BigIndianInt(payload, payloadLen - 3, 3) != 9)
                return;


            AudioCodecs = new List<KeyValuePair<AudioCodec, bool>>();
            VideoCodecs = new List<KeyValuePair<VideoCodec, bool>>();
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
                AudioCodecs.Add( new KeyValuePair<AudioCodec, bool>(codec, false ));

            foreach (var codec in videoCodecs)
                VideoCodecs.Add(new KeyValuePair<VideoCodec, bool>(codec, false));
        }

        public UInt32 TransactionID
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
        public List<KeyValuePair<AudioCodec,bool>> AudioCodecs
        {
            get;
            set;
        }
        public List<KeyValuePair<VideoCodec,bool>> VideoCodecs
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
