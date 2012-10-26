using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace rtmpproxy.Messages
{
    class ResultSuccess
    {
        private const int clientID = 1;
        public ResultSuccess()
        {
            
        }
        public ResultSuccess(byte[] payload)
        {
            
        }
        public byte[] Serialize()
        {

            var result = new MemoryStream();
            var param0 = new AMFProperty("", "_result", AMF0Types.String).Serialize();
            var param1 = new AMFProperty("", (double)1, AMF0Types.Number).Serialize();

            var object1 = new AMFObject();
            object1.SetProperty("fmsVer", "FMS/3,5,7,7009", AMF0Types.String);
            object1.SetProperty("capabilities", (double)31, AMF0Types.Number);
            object1.SetProperty("mode", (double)1, AMF0Types.Number);

            var object2 = new AMFObject();
            object2.SetProperty("level", "status", AMF0Types.String);
            object2.SetProperty("code", "NetConnection.Connect.Success", AMF0Types.String);
            object2.SetProperty("description", "Connection succeeded.", AMF0Types.String);
            var verArray = new List<AMFProperty>();

            verArray.Add(new AMFProperty( "version", "3,5,7,7009",AMF0Types.String));
            object2.SetProperty("data", verArray, AMF0Types.Array);
            object2.SetProperty("clientid", (double)clientID, AMF0Types.Number);
            object2.SetProperty("objectEncoding", (double)0, AMF0Types.Number);

            var param2 = object1.Serialize();
            var param3 = object2.Serialize();

            var bodysize = param0.Length + param1.Length + param2.Length + param3.Length;
            var header = new Header()
            {
                ChunkStreamID = 3,
                Format = 0,
                StreamID = 0,
                Timestamp = 0,
                TypeID = 0x14,
                BodySize = bodysize
            }.Serialize();

            result.Write(header, 0, header.Length);
            result.Write(param0, 0, param0.Length);
            result.Write(param1, 0, param1.Length);
            result.Write(param2, 0, param2.Length);
            result.Write(param3, 0, param3.Length);

            result.Flush();
            return result.ToArray();
        }
    }
}
