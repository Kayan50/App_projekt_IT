namespace App_projekt_IT.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Voivodeship { get; set; }

        public ICollection<Clinic> Clinics { get; set; }
    }
}
