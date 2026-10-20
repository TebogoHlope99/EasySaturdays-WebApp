using System.ComponentModel.DataAnnotations;

namespace EasySaturdays.Models
{
    public class RoleChangeRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string CurrentRole { get; set; }

        [Required]
        public string RequestedRole { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}