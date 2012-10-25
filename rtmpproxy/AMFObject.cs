using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
            var lengthBytes = ArrayUtil.Right(BitConverter.GetBytes(Name.Length).Reverse().ToArray(), 2);
            result.Write( lengthBytes, 0, lengthBytes.Length);             
            
            var nameBytes = Encoding.ASCII.GetBytes(Name);
            result.Write( nameBytes, 0, nameBytes.Length );

            switch (Type)
            {
                case AMF0Types.Number:
                    double val = (double)Value;
                    param = ArrayUtil.ConcatArrays(new byte[] { 0x00 }, ArrayUtil.Right( BitConverter.GetBytes(val).Reverse().ToArray(), 2));
                    break;
                case AMF0Types.Boolean:                  
                    param = new byte[] { 0x01, (bool)Value ? (byte)1 : (byte)0 };
                    break;
                case AMF0Types.String:
                    string stringVal = (string)Value;
                    param = ArrayUtil.ConcatArrays(new byte[] { 0x02 }, ArrayUtil.Right( BitConverter.GetBytes(stringVal.Length).Reverse().ToArray(), 2 ));
                    param = ArrayUtil.ConcatArrays(param, Encoding.ASCII.GetBytes(stringVal));
                    break;
                case AMF0Types.Array:                    
                    List<AMFProperty> array = (List<AMFProperty>)Value;
                    var count = array.Count - 1;
                    param = ArrayUtil.ConcatArrays( new byte[] { 0x08}, ArrayUtil.Right( BitConverter.GetBytes(count).Reverse().ToArray(), 2));
                    foreach( var v in array )
                        param = ArrayUtil.ConcatArrays( param, v.Serialize() );
                    param = ArrayUtil.ConcatArrays( param, new byte[] {0,0,9} );
                    break;
            }
            result.Write( param, 0, param.Length );
            result.Flush();
            var resultBytes = result.ToArray();
            Length = resultBytes.Length;

            return resultBytes;
        }

    }
    class AMFObject
    {
        private List<AMFProperty> _properties;
        public AMFObject()
        {
            _properties = new List<AMFProperty>();
        }
        
        public AMFObject(byte[] data)
        {
            int paramNameLength = 0;
            int paramValueLength = 0;
            int dataLength = data.Length;
            string paramName = null;
            _properties = new List<AMFProperty>();
            Debug.Print("AMF object:");
            for (int i = 0; i < dataLength; i++)
            {
                paramNameLength = (int)ArrayUtil.BigIndianInt(data,i,2);
                i += 2;
                if (paramNameLength > 0)
                {
                    byte[] byteName = ArrayUtil.Mid(data,i,paramNameLength);
                    paramName = Encoding.ASCII.GetString(byteName);
                    i += paramNameLength;
                    
                    object paramValue = null;
                    
                    var amfType = (AMF0Types)data[i];

                    switch (amfType)
                    {
                        case AMF0Types.String:
                            paramValueLength = (int)ArrayUtil.BigIndianInt(data, i + 1, 2);
                            paramValue = Encoding.ASCII.GetString(ArrayUtil.Mid(data, i + 3, paramValueLength));
                            _properties.Add(new AMFProperty( paramName,  paramValue, AMF0Types.String));
                            Debug.Print(String.Format("{0} = {1}", paramName, (String)paramValue));
                            i += paramValueLength + 2;
                            break;
                        case AMF0Types.Boolean:
                            paramValueLength = 1;
                            paramValue = data[i + 1];
                            _properties.Add(new AMFProperty( paramName,  paramValue, AMF0Types.Boolean));
                            Debug.Print(String.Format("{0} = {1}", paramName, (Boolean)paramValue));
                            i += paramValueLength;
                            break;
                        case AMF0Types.Number:
                            double result = 0;
                            paramValueLength = 4;
                            if (ArrayUtil.AMF0Number(data, i + 1, ref result))
                            {
                                paramValue = result;
                                _properties.Add(new AMFProperty(paramName, paramValue, AMF0Types.Number));
                                Debug.Print(String.Format("{0} = {1}", paramName, (float)paramValue));
                                i += paramValueLength;
                            }
                            break;
                    }   
                    
                }
            }
        }
        public int Length
        {
            get;
            set;
        }
        public void SetProperty(string name, object value, AMF0Types type)
        {
            var prop = _properties.FirstOrDefault( n => n.Name == name );
            if (prop == null)
                _properties.Add(new AMFProperty(name, value, type));
            else
                _properties.Add(prop);
        }
        public object GetProperty(string name)
        {
            var prop = _properties.FirstOrDefault(n => n.Name == name);
            if (prop == null)
                return null;
            else
                return prop.Value;
        }
        public byte[] Serialize()
        {
            var result = new byte[] { 0x03 };
            foreach (var p in _properties)
                result = ArrayUtil.ConcatArrays(result, p.Serialize());

            result = ArrayUtil.ConcatArrays(result, new byte[] { 0x00, 0x00, 0x09 });
            Length = result.Length;
            return result;
        }
    }
}
