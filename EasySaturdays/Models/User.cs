using System.ComponentModel.DataAnnotations;

namespace EasySaturdays.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string PasswordHash { get; set; }
        [Required] public string Role { get; set; } // Admin, Donor, Volunteer
        public string? Phone { get; set; }
        public string? Skills { get; set; }
    }
}