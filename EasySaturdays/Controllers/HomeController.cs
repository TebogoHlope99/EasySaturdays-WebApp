using EasySaturdays.Data;
using EasySaturdays.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasySaturdays.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context) { _context = context; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Campaigns()
        {
            return View(_context.Campaigns.Where(c => c.Status == "Active").ToList());
        }

        [Authorize]
        [HttpGet]
        public IActionResult VolunteerDetail(int id)
        {
            var campaign = _context.Campaigns.Find(id);
            if (campaign == null) return NotFound();
            return View(campaign);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SignUpShift(int campaignId, DateTime date, string time)
        {
            // SECURITY CHECK: Role validation
            if (!User.IsInRole("Volunteer") && !User.IsInRole("Both") && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "You must be registered as a Volunteer to sign up for shifts." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // RULE 1: Check if the user already has a shift on this SPECIFIC DATE (regardless of campaign)
            bool hasShiftOnDate = _context.VolunteerShifts
                .Any(s => s.UserId == userId && s.Date.Date == date.Date);

            if (hasShiftOnDate)
            {
                return Json(new { success = false, message = "You already have a volunteer shift scheduled for this day. You can only do one shift per day." });
            }

            // RULE 2: Check if they already signed up for THIS EXACT shift (Safety check)
            if (_context.VolunteerShifts.Any(s => s.UserId == userId && s.CampaignId == campaignId && s.Date == date && s.Time == time))
            {
                return Json(new { success = false, message = "You are already signed up for this specific shift." });
            }

            // Save to DB
            _context.VolunteerShifts.Add(new VolunteerShift
            {
                UserId = userId,
                CampaignId = campaignId,
                Date = date,
                Time = time,
                Status = "Upcoming"
            });
            _context.SaveChanges();

            return Json(new { success = true, message = "Shift successfully registered!" });
        }

        [Authorize]
        [HttpPost]
        public IActionResult CancelShift(int shiftId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var shift = _context.VolunteerShifts.FirstOrDefault(s => s.Id == shiftId && s.UserId == userId);

            if (shift == null) return Json(new { success = false, message = "Shift not found." });

            // RULE 3: Can only cancel 1 day (24 hours) before the shift
            if (shift.Date.Date <= DateTime.Today)
            {
                return Json(new { success = false, message = "You cannot cancel a shift on the day of the shift. Please contact the administrator." });
            }

            _context.VolunteerShifts.Remove(shift);
            _context.SaveChanges();

            return Json(new { success = true, message = "Shift successfully cancelled." });
        }

        //Payements
        [Authorize]
        [HttpGet]
        public IActionResult Payment(int id)
        {
            // SECURITY CHECK: Role validation (Fail fast before loading page)
            if (!User.IsInRole("Donor") && !User.IsInRole("Both") && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You must be registered as a Donor to make a donation.";
                return RedirectToAction("Campaigns");
            }

            var campaign = _context.Campaigns.Find(id);
            if (campaign == null || campaign.Status == "Ended") return NotFound();

            ViewBag.Remaining = campaign.GoalAmount - campaign.RaisedAmount;
            return View(campaign);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ProcessPayment(int campaignId, decimal amount, string method, string cardNumber)
        {
            // SECURITY CHECK: Role validation (Backend catch-all)
            if (!User.IsInRole("Donor") && !User.IsInRole("Both") && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "You must be registered as a Donor to make a donation." });
            }

            var campaign = _context.Campaigns.Find(campaignId);
            if (campaign == null) return Json(new { success = false, message = "Campaign not found." });

            var remaining = campaign.GoalAmount - campaign.RaisedAmount;

            // Server-side clamping and validation
            if (amount < 50) return Json(new { success = false, message = "Minimum donation is R50." });
            if (amount > remaining) return Json(new { success = false, message = $"Amount exceeds remaining goal (R{remaining})." });
            if (method != "Cash" && (string.IsNullOrEmpty(cardNumber) || cardNumber.Replace(" ", "").Length != 16))
                return Json(new { success = false, message = "Invalid card number." });

            // Record Donation
            var donation = new Donation
            {
                Amount = amount,
                Method = method,
                CampaignId = campaignId,
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
            };
            _context.Donations.Add(donation);

            // Increment Campaign
            campaign.RaisedAmount += amount;

            // Auto-end campaign if goal reached
            if (campaign.RaisedAmount >= campaign.GoalAmount)
                campaign.Status = "Ended";

            _context.SaveChanges();
            return Json(new { success = true, message = $"Thank you! You donated R{amount}." });
        }
    }
}