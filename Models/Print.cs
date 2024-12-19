using System.ComponentModel.DataAnnotations;

namespace PrinterSystem.Models
{
    public class Print
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int FDb { get; set; }
        [Required]
        public int FFDb { get; set; }
        [Required]
        public int SzDb { get; set; }
        [Required]
        public int SzszDb { get; set; }
        [Required]
        public int ScDb { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
