using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RpgServer.Models
{
    [Table("Device")]
    public class DeviceModel
    {
        public enum EState
        {
            Disconnected,
            Connected,
        }

        [Key]
        public string Idfv { get; set; } = string.Empty;

        public EState State { get; set; }

        public ulong AccountId { get; set; }
        
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;
        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
