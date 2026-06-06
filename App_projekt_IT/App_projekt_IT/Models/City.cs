using System.ComponentModel.DataAnnotations;

namespace App_projekt_IT.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podanie nazwy miasta jest wymagane.")]
        [Display(Name = "Nazwa Miasta")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Podanie województwa jest wymagane.")]
        [Display(Name = "Województwo")]
        public string Voivodeship { get; set; } 

        public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    }
}