using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Bulky.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public Address Address { get; set; }
        
        [NotMapped]
        public string Role { get; set; }
    }
}