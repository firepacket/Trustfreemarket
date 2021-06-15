using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AnarkRE.Security
{
    public static class MyCrypto
    {
        public static int GenerateRandomNumber(int minValue, int maxValueInc)
        {
            byte[] randomBytes = new byte[4];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            int seed = ((randomBytes[0] & 0x7f) << 24) |
                        (randomBytes[1] << 16) |
                        (randomBytes[2] << 8) |
                        (randomBytes[3]);

            Random random = new Random(seed);

            return random.Next(minValue, maxValueInc + 1);
        }

        public static byte[] GenerateRandomData(int length)
        {
            byte[] d = new byte[length];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(d);
            return d;
        }

        public static string GenerateRandomString(int length)
        {
            string allowed = "abcdefghijklmnopqrstuvwxyxABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder sb = new StringBuilder(0, length);
            while (sb.Length < length)
                sb.Append(allowed[GenerateRandomNumber(0, allowed.Length-1)]);
            return sb.ToString();
        }

        public static string MD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = ASCIIEncoding.Default.GetBytes(str);
            byte[] encoded = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < encoded.Length; i++)
                sb.Append(encoded[i].ToString("x2"));

            return sb.ToString();
        }
    }
}