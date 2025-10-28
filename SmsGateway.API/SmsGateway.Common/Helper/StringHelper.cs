using System.Security.Cryptography;
using System.Text;

namespace SmsGateway.Common.Helper
{
    public static class StringHelper
    {
        public static string GenerateTimeBasedCorrelationId()
        {
            long unixTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            string unixTimestampString = unixTimestampSeconds.ToString();
            return $"TheGrandHoTram_" + unixTimestampString;
        }

        public static string GenerateNumericCode(int length)
        {
            // Cryptographically strong numeric code
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes)
            {
                sb.Append(b % 10);
            }
            return sb.ToString();
        }
    }
}