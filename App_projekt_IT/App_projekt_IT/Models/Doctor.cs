namespace App_projekt_IT.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }

        // Klucz obcy do Szpitala
        public int ClinicId { get; set; }
        public Clinic Clinic { get; set; }

        public ICollection<Service> Services { get; set; }
        public ICollection<AppointmentSlot> AppointmentSlots { get; set; }
    }

}
