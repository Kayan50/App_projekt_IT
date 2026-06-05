using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace App_projekt_IT.Models
{
    public class Clinic
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa kliniki jest wymagana.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Wprowadź poprawny numer telefonu.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Wprowadź poprawny format adresu e-mail.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres jest wymagany.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Musisz wybrać miasto.")]
        public int CityId { get; set; }

        [ValidateNever] 
        public City City { get; set; }

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}