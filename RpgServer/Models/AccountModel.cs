using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RpgServer.Models
{
    [MessagePack.MessagePackObject]
    [Table("Account")]
    public class AccountModel
    {
        [MessagePack.Key(1)]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [MessagePack.Key(2)]
        public string Idfv {  get; set; } = string.Empty;

        [MessagePack.Key(3)]
        public string SessionId { get; set; } = string.Empty;

        [MessagePack.Key(0)]
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;

        [MessagePack.IgnoreMember]
        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
