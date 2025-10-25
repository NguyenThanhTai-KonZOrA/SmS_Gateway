namespace SmsGateway.Common.Helper
{
    public static class StringHelper
    {
        public static string GenerateTimeBasedCorrelationId()
        {
            long unixTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            string unixTimestampString = unixTimestampSeconds.ToString();
            return $"TheGrandHT_" + unixTimestampString;
        }
    }
}