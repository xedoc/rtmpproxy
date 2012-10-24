using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rtmpproxy
{
    class AMFObject
    {
        private Dictionary<string, object> _properties;
        public AMFObject(byte[] data)
        {
            int paramNameLength = 0;
            int paramValueLength = 0;
            int dataLength = data.Length;
            string paramName = null;
            _properties = new Dictionary<string, object>();
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
                            _properties.Add(paramName, paramValue);
                            Debug.Print(String.Format("{0} = {1}", paramName, (String)paramValue));
                            i += paramValueLength + 2;
                            break;
                        case AMF0Types.Boolean:
                            paramValueLength = 1;
                            paramValue = data[i + 1];
                            _properties.Add(paramName, paramValue);
                            Debug.Print(String.Format("{0} = {1}", paramName, (Boolean)paramValue));
                            i += paramValueLength;
                            break;
                        case AMF0Types.Number:
                            double result = 0;
                            paramValueLength = 4;
                            if (ArrayUtil.AMF0Number(data, i + 1, ref result))
                            {
                                paramValue = result;
                                _properties.Add(paramName, paramValue);
                                Debug.Print(String.Format("{0} = {1}", paramName, (float)paramValue));
                                i += paramValueLength;
                            }
                            break;
                    }   
                    
                }
            }
        }
        public void SetProperty(string name, object value)
        {
            if (_properties.Keys.FirstOrDefault(n => n == name) == null)
                _properties.Add(name, value);
            else
                _properties[name] = value;
        }
        public object GetProperty(string name)
        {
            object result;
            _properties.TryGetValue( name, out result );
            return result;
        }
    }
}
