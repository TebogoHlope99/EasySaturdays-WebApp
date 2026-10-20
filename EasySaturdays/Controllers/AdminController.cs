using EasySaturdays.Data;
using EasySaturdays.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasySaturdays.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        // 1. MAIN DASHBOARD
        public IActionResult Index()
        {
            var users = _context.Users.ToList();

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalDonations = _context.Donations.Sum(d => d.Amount);
            ViewBag.Volunteers = users.Count(u => u.Role == "Volunteer" || u.Role == "Both");
            ViewBag.ActiveVolunteers = _context.VolunteerShifts.Select(v => v.UserId).Distinct().Count();

            ViewBag.Users = _context.Users.OrderBy(u => u.FirstName).ToList();

            ViewBag.Donations = _context.Donations
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.Date)
                .ToList();

            ViewBag.Shifts = _context.VolunteerShifts
                .Include(s => s.User)
                .Include(s => s.Campaign)
                .OrderByDescending(s => s.Date)
                .ToList();

            return View(_context.Campaigns.ToList());
        }

        // 2. CREATE CAMPAIGN (POST)
        [HttpPost]
        public IActionResult CreateCampaign(Campaign campaign)
        {
            if (ModelState.IsValid)
            {
                campaign.Status = "Active";
                campaign.RaisedAmount = 0;
                _context.Campaigns.Add(campaign);
                _context.SaveChanges();
                TempData["Success"] = "Campaign created successfully!";
            }
            return RedirectToAction("Index");
        }

        // 3. EDIT CAMPAIGN (GET - Loads the form)
        [HttpGet]
        public IActionResult EditCampaign(int id)
        {
            var campaign = _context.Campaigns.Find(id);
            if (campaign == null) return NotFound();
            return View(campaign);
        }

        // 4. EDIT CAMPAIGN (POST - Saves changes)
        [HttpPost]
        public IActionResult EditCampaign(Campaign campaign)
        {
            if (ModelState.IsValid)
            {
                _context.Campaigns.Update(campaign);
                _context.SaveChanges();
                TempData["Success"] = "Campaign updated successfully!";
                return RedirectToAction("Index");
            }
            return View(campaign);
        }

        // 5. DELETE CAMPAIGN (POST)
        [HttpPost]
        public IActionResult DeleteCampaign(int id)
        {
            var campaign = _context.Campaigns.Find(id);
            if (campaign != null)
            {
                _context.Campaigns.Remove(campaign);
                _context.SaveChanges();
                TempData["Success"] = "Campaign deleted.";
            }
            return RedirectToAction("Index");
        }

        // 6. DELETE USER (POST)
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            // Prevent deleting yourself or the main admin
            if (user != null && user.Email != "admin@easysaturdays.com")
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = "User deleted.";
            }
            return RedirectToAction("Index");
        }

        // 7. VIEW REPORTS (GET - Shows the Report Page)
        [HttpGet]
        public IActionResult ViewReports()
        {
            var users = _context.Users.ToList();
            ViewBag.TotalUsers = users.Count;
            ViewBag.Donors = users.Count(u => u.Role == "Donor" || u.Role == "Both");
            ViewBag.Volunteers = users.Count(u => u.Role == "Volunteer" || u.Role == "Both");
            ViewBag.ActiveVolunteers = _context.VolunteerShifts.Select(v => v.UserId).Distinct().Count();

            var donations = _context.Donations.ToList();
            ViewBag.TotalDonations = donations.Sum(d => d.Amount);
            ViewBag.AvgDonation = donations.Any() ? donations.Average(d => d.Amount) : 0;

            ViewBag.TotalShifts = _context.VolunteerShifts.Count();
            ViewBag.ActiveCampaigns = _context.Campaigns.Count(c => c.Status == "Active");

            return View();
        }
    }
}