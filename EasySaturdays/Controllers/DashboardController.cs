using EasySaturdays.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasySaturdays.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) { _context = context; }

        public IActionResult Index()
        {
            // 1. Admin Redirect 
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            // 2. Get User Data
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role); // Donor, Volunteer, or Both

            // 3. Load Donations & Shifts
            var donations = _context.Donations
                .Include(d => d.Campaign)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.Date)
                .ToList();

            var shifts = _context.VolunteerShifts
                .Include(s => s.Campaign)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Date)
                .ToList();

            // 4. Calculate Donor Stats
            ViewBag.TotalDonated = donations.Sum(d => d.Amount);
            ViewBag.CampaignsSupported = donations.Select(d => d.CampaignId).Distinct().Count();
            ViewBag.TaxReceipts = donations.Count; // Assuming 1 receipt per donation
            ViewBag.RecentDonations = donations.Take(5).ToList();

            // 5. Calculate Volunteer Stats
            // For "Hours Contributed", we'll mock 4 hours per completed shift
            ViewBag.HoursContributed = shifts.Count(s => s.Status == "Completed") * 4;
            ViewBag.ShiftsCompleted = shifts.Count(s => s.Status == "Completed");
            ViewBag.UpcomingShiftsCount = shifts.Count(s => s.Status == "Upcoming");
            ViewBag.UpcomingShifts = shifts.Where(s => s.Status == "Upcoming").ToList();
            ViewBag.RecentShifts = shifts.Take(5).ToList();

            // Pass the role to the view so we know which layout to show
            ViewBag.UserRole = userRole;

            return View();
        }
    }
}