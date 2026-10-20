using System.ComponentModel.DataAnnotations;

namespace EasySaturdays.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        [Required, Range(50, 10000000)] public decimal GoalAmount { get; set; }
        public decimal RaisedAmount { get; set; } = 0;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);
        public string Status { get; set; } = "Active"; // Active, Ended
        public string Location { get; set; } = "Tembisa";
        public string SkillsNeeded { get; set; } = "General Support";
    }
}