using Microsoft.AspNetCore.Identity;
using App_projekt_IT.Models; 

namespace App_projekt_IT.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Pobranie serwisów do zarządzania rolami i użytkownikami
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tworzenie ról w systemie
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Patient"))
            {
                await roleManager.CreateAsync(new IdentityRole("Patient"));
            }

            // 2. Tworzenie konta administratora
            string adminEmail = "admin@klinika.pl";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, 
                    FirstName = "Główny",
                    LastName = "Administrator",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    PESEL = "00000000000"
                };

                
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    // Przypisanie nowo utworzonego konta do roli Admin
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}
