using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RpgServer.Models
{
    [Table("Account")]
    public class AccountModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        public string Idfv {  get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        
        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;
        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
