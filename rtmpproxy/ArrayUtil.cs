using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rtmpproxy
{
    static class ArrayUtil
    {
        public static byte[] ConcatArrays(byte[] leftArray, byte[] rightArray)
        {
            int leftLength = leftArray.Length;
            int rightLength = rightArray.Length;

            byte[] resultArray = new byte[leftLength + rightLength];
            if (leftLength > 0)
            {
                Buffer.BlockCopy(leftArray, 0, resultArray, 0, leftLength);
                Buffer.BlockCopy(rightArray, 0, resultArray, leftLength, rightLength);
            }
            else
            {
                resultArray = rightArray;
            }

            return resultArray;
        }
        public static byte[] Right(byte[] srcArray, int startIndex)
        {
            if (startIndex == 0)
                return srcArray;

            int resultLength = srcArray.Length - startIndex;

            if (resultLength <= 0)
                return new byte[0];

            byte[] resultArray = new byte[resultLength];
            Buffer.BlockCopy(srcArray, startIndex, resultArray, 0, resultLength);

            return resultArray;
        }
        public static byte[] Mid(byte[] srcArray, int startIndex, int Count)
        {
            if (startIndex < 0)
                return srcArray;
            if (Count <= 0)
                return new byte[0];

            byte[] resultArray = new byte[Count];
            Buffer.BlockCopy(srcArray, startIndex, resultArray, 0, Count);

            return resultArray;
        }        
        public static UInt32 BigIndianInt(byte[] srcArray, int startIndex, int bytesCount)
        {
            uint resultValue = 0;
            var shiftBits = (bytesCount - 1) * 8;
            for (int i = 0; i < bytesCount; i++)
            {
                resultValue += ((uint)srcArray[startIndex + i] << shiftBits);
                shiftBits -= 8;
            }
            return resultValue;
        }
        public static bool AMF0Number(byte[] srcArray, int startIndex, ref float result )
        {
            byte[] slice = Right(srcArray, startIndex);
            if (slice == null)
                return false;
            if (slice.Length <= 3)
                return false;
            
            result = BitConverter.ToSingle( slice.Reverse().ToArray(), startIndex );
            return true;
        }
        public static String AMF0String(byte[] srcArray, int startIndex)
        {
            byte[] slice = Right(srcArray, startIndex );

            if (slice == null)
                return null;
            if (slice.Length <= 3)
                return null;

            if( slice[0] != (byte)AMF0Types.String )
                return null;

            var length = BigIndianInt(slice, 1, 2);

            if (slice.Length < (3 + length))
                return null;

            return Encoding.ASCII.GetString(slice,3,(int)length);
            
        }
    }
}
