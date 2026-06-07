using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App_projekt_IT.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } 

        [Required]
        public int AppointmentSlotId { get; set; } 

        [ForeignKey("AppointmentSlotId")]
        public AppointmentSlot AppointmentSlot { get; set; }

        [Required(ErrorMessage = "Ocena jest wymagana.")]
        [Range(1, 5, ErrorMessage = "Ocena musi być w przedziale od 1 do 5 gwiazdek.")]
        [Display(Name = "Ocena")]
        public int Rating { get; set; } 

        [Display(Name = "Komentarz (opcjonalny)")]
        [MaxLength(1000, ErrorMessage = "Komentarz nie może przekraczać 1000 znaków.")]
        public string? Comment { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}