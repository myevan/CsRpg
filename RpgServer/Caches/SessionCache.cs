namespace RpgServer.Contexts
{
    public class SessionCache
    {
        public ulong AccountId { get; set; } = 0;
        public ulong PlayerId { get; set; } = 0;
        public string Idfv { get; set; } = string.Empty;
        public string Idfa { get; set; } = string.Empty;

        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
