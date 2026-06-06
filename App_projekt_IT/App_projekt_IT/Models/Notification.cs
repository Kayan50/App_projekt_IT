using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App_projekt_IT.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        
        public int? AppointmentSlotId { get; set; }
        [ForeignKey("AppointmentSlotId")]
        public AppointmentSlot AppointmentSlot { get; set; }

        
        public string Type { get; set; } = "Info";
    }
}