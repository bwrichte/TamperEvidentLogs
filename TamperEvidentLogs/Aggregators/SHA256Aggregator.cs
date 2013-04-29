
using System.Security.Cryptography;

namespace TamperEvidentLogs.Aggregators
{
    public class SHA256Aggregator : Aggregator
    {
        public static HMACSHA256 GetHMAC(byte tag)
        {
            return new HMACSHA256(new byte[] { tag });
        }

        public byte[] AggregateChildren(byte[] left, byte[] right)
        {
            if (left != null && right != null)
            {
                using (HMACSHA256 hmac = GetHMAC((byte)1))
                {
                    hmac.TransformBlock(left, 0, left.Length, left, 0);
                    hmac.TransformFinalBlock(right, 0, right.Length);
                    return hmac.Hash;
                }
            }
            else if (left != null)
            {
                using (HMACSHA256 hmac = GetHMAC((byte)2))
                {
                    return hmac.ComputeHash(left);
                }
            }
            else if (right != null)
            {
                using (HMACSHA256 hmac = GetHMAC((byte)3))
                {
                    return hmac.ComputeHash(right);
                }
            }
            else
            {
                return null;
            }
        }

        public byte[] HashLeaf(byte[] data)
        {
            using (HMACSHA256 hmac = GetHMAC((byte)4))
            {
                return hmac.ComputeHash(data);
            }
        }

        public string Name
        {
            get
            {
                return "SHA256";
            }
        }
    }
}
