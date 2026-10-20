using System.ComponentModel.DataAnnotations;

namespace EasySaturdays.Models
{
    public class Donation
    {
        public int Id { get; set; }
        [Required, Range(50, 1000000)] public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public string Method { get; set; } = "Credit Card"; // Card, EFT, Mobile, Cash

        public int UserId { get; set; }
        public User User { get; set; }

        public int CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}