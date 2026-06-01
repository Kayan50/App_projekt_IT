using App_projekt_IT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace App_projekt_IT.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<AppointmentSlot> AppointmentSlots { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // KRYTYCZNE: Ta linijka musi zostać! Bez niej cały system Identity (logowanie) się posypie.
            base.OnModelCreating(builder);

            // 1. Zasilenie tabeli Miast
            builder.Entity<City>().HasData(
                new City { Id = 1, Name = "Kraków", Voivodeship = "Małopolskie" },
                new City { Id = 2, Name = "Warszawa", Voivodeship = "Mazowieckie" }
            );

            // 2. Zasilenie tabeli Klinik (Pamiętaj o podaniu CityId, żeby utworzyć relację)
            builder.Entity<Clinic>().HasData(
                new Clinic { Id = 1, Name = "Centrum Medyczne Zdrowie", CityId = 1, Address = "ul. Karmelicka 10", PostalCode = "31-128", Phone = "123456789", Email = "kontakt@zdrowie.pl" },
                new Clinic { Id = 2, Name = "Prywatna Lecznica", CityId = 2, Address = "ul. Nowy Świat 5", PostalCode = "00-029", Phone = "987654321", Email = "biuro@lecznica.pl" }
            );

            // 3. Zasilenie tabeli Usług
            builder.Entity<Service>().HasData(
                new Service { Id = 1, Name = "Konsultacja kardiologiczna", IsNFZ = true },
                new Service { Id = 2, Name = "Rezonans magnetyczny", IsNFZ = false },
                new Service { Id = 3, Name = "Konsultacja ortopedyczna", IsNFZ = true }
            );

            // 4. Zasilenie tabeli Lekarzy (Zauważ, że podpinamy ich pod ClinicId)
            builder.Entity<Doctor>().HasData(
                new Doctor { Id = 1, FirstName = "Jan", LastName = "Kowalski", Title = "lek. med.", ClinicId = 1 },
                new Doctor { Id = 2, FirstName = "Anna", LastName = "Nowak", Title = "dr n. med.", ClinicId = 1 },
                new Doctor { Id = 3, FirstName = "Piotr", LastName = "Wiśniewski", Title = "lek. med.", ClinicId = 2 }
            );
        }
    }
}
