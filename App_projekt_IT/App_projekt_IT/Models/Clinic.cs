namespace App_projekt_IT.Models
{
    public class Clinic
    {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }

    // Klucz obcy do Miasta
    public int CityId { get; set; }
    public City City { get; set; }

    public ICollection<Doctor> Doctors { get; set; }
}
}
