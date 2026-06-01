using App_projekt_IT.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace App_projekt_IT.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PESEL { get; set; }

        public ICollection<AppointmentSlot> Appointments { get; set; }
    }
}