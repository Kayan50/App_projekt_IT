using System;
using System.ComponentModel.DataAnnotations;

namespace App_projekt_IT.Models.ViewModels
{
    public class ScheduleGeneratorViewModel
    {
        [Required]
        [Display(Name = "Lekarz")]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Usługa")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Wymagana data usługi")]
        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Wymagana godzina rozpoczęcia.")]
        [DataType(DataType.Time)]
        [Display(Name = "Godzina rozpoczęcia")]
        public TimeSpan? StartTime { get; set; }

        [Required(ErrorMessage = "Wymagana godzina zakończenia.")]
        [DataType(DataType.Time)]
        [Display(Name = "Godzina zakończenia")]
        public TimeSpan? EndTime { get; set; }

        [Required]
        [Display(Name = "Czas trwania wizyty (minuty)")]
        public int IntervalMinutes { get; set; }
    }
}