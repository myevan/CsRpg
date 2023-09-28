using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RpgServer.Models
{
    [Index(nameof(Id), IsUnique = true)]
    [Table("Session")]
    public class SessionModel
    {
        
        [Key]
        public ulong AccountId { get; set; } = 0;

        public ulong PlayerId { get; set; } = 0;

        public string Id { get; set; } = string.Empty;
        public string Idfv { get; set; } = string.Empty;
        public string Idfa { get; set; } = string.Empty;

        public DateTime CreateTime { get; private set; } = DateTime.UtcNow;
        public DateTime UpdateTime { get; private set; } = DateTime.UtcNow;
    }
}
