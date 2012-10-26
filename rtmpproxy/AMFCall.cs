using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using rtmpproxy.Messages;

namespace rtmpproxy
{
    class AMFCall
    {
        private List<AMFProperty> _properties;
        public AMFCall(string name, List<AMFProperty> properties)
        {
            Name = name;
            Properties = properties;
        }
        public AMFCall(byte[] payload)
        {
            Name = "";
            _properties = new List<AMFProperty>();
            for (int i = 0; i < payload.Length; i++)
            {
                var prop = new AMFProperty(payload, i);
                _properties.Add(prop);
                i += prop.Length - 1;
            }            
        }
        public string Name
        {
            get;
            set;
        }
    
        public List<AMFProperty> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }
        public int Length
        {
            get;
            set;
        }
        public byte[] Serialize()
        {
            var result = new MemoryStream();            
            var parameters = new byte[0];
            
            foreach (var p in Properties)
                parameters = ArrayUtil.ConcatArrays(parameters, p.Serialize());

            var resultTitle = new AMFProperty("", Name, AMF0Types.String).Serialize();

            var header = new Header()
            {
                ChunkStreamID = 3,
                Format = 0,
                StreamID = 0,
                Timestamp = 0,
                TypeID = 0x14,
                BodySize = parameters.Length + resultTitle.Length
            }.Serialize();
            
            result.Write(header, 0 ,header.Length);
            result.Write( resultTitle, 0, resultTitle.Length  );
            result.Write(parameters, 0, parameters.Length);

            result.Flush();

            var byteResult = result.ToArray();
            Length = byteResult.Length;
            return byteResult;
        }
    }
}
