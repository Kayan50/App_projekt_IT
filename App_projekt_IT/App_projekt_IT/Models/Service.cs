using System.ComponentModel.DataAnnotations;

namespace App_projekt_IT.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa usługi jest wymagana.")]
        public string Name { get; set; } = string.Empty;

        public bool IsNFZ { get; set; }

       
        [MaxLength(2000, ErrorMessage = "Opis nie może przekraczać 2000 znaków.")]
        public string? Description { get; set; } 
        
        public string? ImagePath { get; set; } 

        public bool IsHighlighted { get; set; } = false; 

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}