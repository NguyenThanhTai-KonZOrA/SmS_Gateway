namespace SmsGateway.Common.Helper
{
    public static class ErrorrMessageMapper
    {
        public static string GetMessage(Dictionary<string, string> listErrors, string code)
        {
            return listErrors.TryGetValue(code, out var message)
                ? message
                : "Unknown error";
        }
    }
}
