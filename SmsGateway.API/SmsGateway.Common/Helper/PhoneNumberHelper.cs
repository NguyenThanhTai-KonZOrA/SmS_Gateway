namespace SmsGateway.Common.Helper
{
    public static class PhoneNumberHelper
    {
        // Standard: "+84xxxxxxxxx", "0084xxxxxxxxx", "0xxxxxxxxx" => "84xxxxxxxxx"
        public static string NormalizeTo84(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var digits = new string(input.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("0084"))
                return "84" + digits[4..];

            if (digits.StartsWith("84"))
                return digits;

            // 0... -> 84...
            if (digits.StartsWith("0") && digits.Length > 1)
                return "84" + digits[1..];

            return digits;
        }
    }
}