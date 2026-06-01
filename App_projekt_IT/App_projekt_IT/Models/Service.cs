namespace App_projekt_IT.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsNFZ { get; set; } // Opcja NFZ/Prywatnie do filtra

        public ICollection<Doctor> Doctors { get; set; }
    }
}
