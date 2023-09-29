using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RpgServer.Models
{
    [MessagePack.MessagePackObject]
    [Index(nameof(Id), IsUnique = true)]
    [Table("Session")]
    public class SessionModel
    {
        [MessagePack.Key(1)]
        [Key]
        public ulong AccountId { get; set; } = 0;

        [MessagePack.Key(2)]
        public ulong PlayerId { get; set; } = 0;

        [MessagePack.Key(3)]
        public string Id { get; set; } = string.Empty;

        [MessagePack.Key(4)]
        public string Idfv { get; set; } = string.Empty;

        [MessagePack.Key(5)]
        public string Idfa { get; set; } = string.Empty;

        [MessagePack.Key(0)]
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;

        [MessagePack.IgnoreMember]

        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
