using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rtmpproxy
{
    class AMFProperty
    {
        public AMFProperty(string name, object value, AMF0Types type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
        public AMFProperty(byte[] payload, int startIndex)
        {
            Type = (AMF0Types)payload[startIndex];
            Name = "";
            Length = 0;
            switch( Type )
            {
                case AMF0Types.String:
                    Value = ArrayUtil.AMF0String(payload,startIndex);
                    Length = ((String)Value).Length + 3;
                    break;
                case AMF0Types.Number:
                    double val = 0;
                    if( ArrayUtil.AMF0Number(payload, startIndex, ref val) )
                        Value = val;
                    else
                        Value = 0;
                    Length = 9;
                    break;
                case AMF0Types.Null:
                    Value = "";
                    Length = 1;
                    break;
            }
        }
        public AMF0Types Type
        {
            get;
            set;
        }
        public String Name
        {
            get;
            set;
        }
        public object Value
        {
            get;
            set;
        }
        public int Length
        {
            get;
            set;
        }
        public byte[] Serialize()
        {
            var nameSize = Name.Length + 2;
            var param = new byte[0];

            var result = new MemoryStream();

            if (!String.IsNullOrEmpty(Name))
            {
                var lengthBytes = ArrayUtil.Right(BitConverter.GetBytes(Name.Length).Reverse().ToArray(), 2);
                result.Write(lengthBytes, 0, lengthBytes.Length);
                var nameBytes = Encoding.ASCII.GetBytes(Name);
                result.Write(nameBytes, 0, nameBytes.Length);
            }

            switch (Type)
            {
                case AMF0Types.Null:
                    param = new byte[] { 0x05 };
                    break;
                case AMF0Types.Number:
                    double val = (double)Value;
                    param = ArrayUtil.ConcatArrays(new byte[] { 0x00 }, BitConverter.GetBytes(val).Reverse().ToArray());
                    break;
                case AMF0Types.Boolean:
                    param = new byte[] { 0x01, (bool)Value ? (byte)1 : (byte)0 };
                    break;
                case AMF0Types.String:
                    string stringVal = (string)Value;
                    param = ArrayUtil.ConcatArrays(new byte[] { 0x02 }, ArrayUtil.Right(BitConverter.GetBytes(stringVal.Length).Reverse().ToArray(), 2));
                    param = ArrayUtil.ConcatArrays(param, Encoding.ASCII.GetBytes(stringVal));
                    break;
                case AMF0Types.Array:
                    List<AMFProperty> array = (List<AMFProperty>)Value;
                    var count = array.Count - 1;
                    param = ArrayUtil.ConcatArrays(new byte[] { 0x08 }, BitConverter.GetBytes(count).Reverse().ToArray());
                    foreach (var v in array)
                        param = ArrayUtil.ConcatArrays(param, v.Serialize());
                    param = ArrayUtil.ConcatArrays(param, new byte[] { 0, 0, 9 });
                    break;
            }
            result.Write(param, 0, param.Length);
            result.Flush();
            var resultBytes = result.ToArray();
            Length = resultBytes.Length;

            return resultBytes;
        }

    }
}
