using System;
using System.Text;
using System.Globalization;

namespace TamperEvidentLogs
{
    public class Encoding
    {
        public static string EncodeBytes(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] DecodeString(string s)
        {
            if (s.Length % 2 != 0)
            {
                throw new ArgumentException(
                    string.Format("The binary string cannot have an odd number of digits: {0}", s)
                    );
            }

            byte[] bytes = new byte[s.Length / 2];
            for (int index = 0; index < bytes.Length; index++)
            {
                string byteValue = s.Substring(index * 2, 2);
                bytes[index] = byte.Parse(
                    byteValue,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                    );
            }

            return bytes;
        }

        public static string Name
        {
            get
            {
                return "Hex";
            }
        }
    }
}
