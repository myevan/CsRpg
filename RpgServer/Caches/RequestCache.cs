namespace RpgServer.Contexts
{
    public class RequestCache
    {
        public string SessionId { get; set; } = string.Empty;
        public ulong Seq { get; set; }
    }
}
