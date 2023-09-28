namespace RpgServer.Extensions
{
    public static class StringExtension
    {
        public static ulong ToUInt64(this string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            if (!ulong.TryParse(value, out ulong result)) return 0;
            return result;
        }
    }
}
