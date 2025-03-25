using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DinhNhatQuynhGiang1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }

        [Display(Name = "Sex")]
        public string? Sex { get; set; }
    }
}
