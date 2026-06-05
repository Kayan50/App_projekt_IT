using System.ComponentModel.DataAnnotations;

namespace App_projekt_IT.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa usługi jest wymagana.")]
        public string Name { get; set; } = string.Empty;

        public bool IsNFZ { get; set; }

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}