using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace EasySaturdays.Models
{
    public class VolunteerShift
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; } // e.g. "08:00 - 12:00"
        public string Status { get; set; } = "Upcoming";
        public int UserId { get; set; }
        public User User { get; set; }
        public int CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}