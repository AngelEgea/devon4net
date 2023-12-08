using System.Security.Cryptography;
using System.Text;

namespace Devon4Net.Infrastructure.Common.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToByteArrayFromHexBinary(this string hexBinary)
        {
            hexBinary.ToByteArrayFromHexBinary(out byte[] result);
            return result;
        }

        public static bool ToByteArrayFromHexBinary(this string hexBinary, out byte[] result)
        {
            try
            {
                if (hexBinary.Length % 2 != 0)
                {
                    result = null;
                    return false;
                }

                result = new byte[hexBinary.Length / 2];

                for (var i = 0; i < hexBinary.Length; i += 2)
                {
                    result[i / 2] = Convert.ToByte($"{hexBinary[i]}{hexBinary[i + 1]}", 16);
                }

                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static string CreateSHA256HashedString(this string inputString)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Fastest method with an increase of memory usage
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ToByteArrayFast(this string hex, out byte[] result)
        {
            try
            {
                if (hex.Length % 2 == 1) throw new ArgumentException("The binary key cannot have an odd number of digits");

                byte[] arr = new byte[hex.Length >> 1];

                for (int i = 0; i < hex.Length >> 1; ++i)
                {
                    arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
                }

                result = arr;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static bool ToByteArrayFromBase64(this string base64string, out byte[] result)
        {
            try
            {
                result = Convert.FromBase64String(base64string);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        private static int GetHexVal(char hex)
        {
            int val = hex;
            if (val < 58)
            {
                return val - 48;
            }

            if (val < 97)
            {
                return val - 55;
            }

            return val - 87;
        }
    }
}
