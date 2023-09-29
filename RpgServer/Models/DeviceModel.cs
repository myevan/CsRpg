using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RpgServer.Models
{
    [MessagePack.MessagePackObject]
    [Table("Device")]
    public class DeviceModel
    {
        public enum EState
        {
            Disconnected,
            Connected,
        }

        [MessagePack.Key(1)]
        [Key]
        public string Idfv { get; set; } = string.Empty;

        [MessagePack.Key(2)]
        public EState State { get; set; }

        [MessagePack.Key(3)]
        public ulong AccountId { get; set; }

        [MessagePack.Key(0)]
        
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;

        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
