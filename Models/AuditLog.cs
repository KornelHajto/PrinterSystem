using System.ComponentModel.DataAnnotations;

namespace PrinterSystem.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Action { get; set; }
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [Required]
        public int UserId { get; set; }
    }
}
