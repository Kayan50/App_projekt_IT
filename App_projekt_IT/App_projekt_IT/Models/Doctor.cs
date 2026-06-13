using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace App_projekt_IT.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Imię lekarza jest wymagane.")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nazwisko lekarza jest wymagane.")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tytuł lekarza jest wymagany.")]
        public string Title { get; set; } = string.Empty;

        public byte[]? ImageData { get; set; }

        public string? ImageContentType { get; set; }


        // Klucz obcy do Szpitala
        public int ClinicId { get; set; }
        [ValidateNever]
        public Clinic Clinic { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
    }

}
