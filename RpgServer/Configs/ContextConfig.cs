namespace RpgServer.Configs
{
    public class ContextConfig
    {
        public string Project { get; private set; } = "RPG";
        public string Region { get; private set; } = "";
        public string App { get; private set; } = "WAS";
        public string Rev { get; private set; } = "${REV}";
    }
}
