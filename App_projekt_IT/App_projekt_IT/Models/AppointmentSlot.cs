namespace App_projekt_IT.Models
{
    public class AppointmentSlot
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsBooked { get; set; }
        
        public bool IsConfirmed { get; set; } = false;
        
        public bool IsReviewed { get; set; } = false;

        // Klucze obce
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }


        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
