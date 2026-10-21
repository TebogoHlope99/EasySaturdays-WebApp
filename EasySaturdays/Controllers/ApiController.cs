using Microsoft.AspNetCore.Mvc;
using EasySaturdays.Data;
using EasySaturdays.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System;

namespace EasySaturdays.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ApiController(AppDbContext context) { _context = context; }

        // --- PUBLIC & USER ENDPOINTS ---
        [HttpGet("campaigns")]
        public IActionResult GetCampaigns() => Ok(_context.Campaigns.ToList());

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginData)
        {
            if (loginData == null || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
                return BadRequest(new { message = "Email and password are required." });

            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == loginData.Email.ToLower());
            if (user != null && BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash))
                return Ok(new { id = user.Id, firstName = user.FirstName, lastName = user.LastName, email = user.Email, role = user.Role, phone = user.Phone, skills = user.Skills });

            return Unauthorized(new { message = "Invalid email or password" });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest regData)
        {
            if (_context.Users.Any(u => u.Email.ToLower() == regData.Email.ToLower()))
                return BadRequest(new { message = "Email is already registered." });

            var newUser = new User
            {
                FirstName = regData.FirstName,
                LastName = regData.LastName,
                Email = regData.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(regData.Password),
                Role = regData.Role,
                Phone = regData.Phone,
                Skills = regData.Skills
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return Ok(new { message = "Registration successful!" });
        }

        [HttpGet("dashboard/{userId}")]
        public IActionResult GetDashboard(int userId)
        {
            var donations = _context.Donations.Where(d => d.UserId == userId).ToList();
            var shifts = _context.VolunteerShifts.Include(s => s.Campaign).Where(s => s.UserId == userId).ToList();

            return Ok(new
            {
                TotalDonated = donations.Sum(d => d.Amount),
                CampaignsSupported = donations.Select(d => d.CampaignId).Distinct().Count(),
                HoursContributed = shifts.Count(s => s.Status == "Completed") * 4,
                ShiftsCompleted = shifts.Count(s => s.Status == "Completed"),
                UpcomingShifts = shifts.Where(s => s.Status == "Upcoming").Select(s => new { id = s.Id, campaignTitle = s.Campaign.Title, date = s.Date.ToString("yyyy-MM-dd"), time = s.Time, status = s.Status }).ToList()
            });
        }

        [HttpPost("profile/update")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest profileData)
        {
            var user = _context.Users.Find(profileData.Id);
            if (user == null) return NotFound();
            user.FirstName = profileData.FirstName; user.LastName = profileData.LastName;
            user.Phone = profileData.Phone; user.Skills = profileData.Skills;
            _context.SaveChanges();
            return Ok(new { message = "Profile updated!" });
        }

        [HttpPost("volunteer/signup")]
        public IActionResult SignUpShift([FromBody] ShiftRequest shiftData)
        {
            bool hasShiftOnDate = _context.VolunteerShifts.Any(s => s.UserId == shiftData.UserId && s.Date.Date == shiftData.Date.Date);
            if (hasShiftOnDate) return BadRequest(new { message = "You already have a shift scheduled for this day." });

            _context.VolunteerShifts.Add(new VolunteerShift { UserId = shiftData.UserId, CampaignId = shiftData.CampaignId, Date = shiftData.Date, Time = shiftData.Time, Status = "Upcoming" });
            _context.SaveChanges();
            return Ok(new { message = "Shift registered!" });
        }

        [HttpPost("volunteer/cancel")]
        public IActionResult CancelShift([FromBody] CancelShiftRequest cancelData)
        {
            var shift = _context.VolunteerShifts.FirstOrDefault(s => s.Id == cancelData.ShiftId && s.UserId == cancelData.UserId);
            if (shift == null) return NotFound();
            _context.VolunteerShifts.Remove(shift);
            _context.SaveChanges();
            return Ok(new { message = "Shift cancelled!" });
        }

        [HttpPost("donate")]
        public IActionResult Donate([FromBody] DonateRequest donateData)
        {
            var campaign = _context.Campaigns.Find(donateData.CampaignId);
            if (donateData.Amount < 50) return BadRequest(new { message = "Minimum donation is R50." });

            campaign.RaisedAmount += donateData.Amount;
            if (campaign.RaisedAmount >= campaign.GoalAmount) campaign.Status = "Ended";

            _context.Donations.Add(new Donation { Amount = donateData.Amount, Method = donateData.Method, CampaignId = donateData.CampaignId, UserId = donateData.UserId });
            _context.SaveChanges();
            return Ok(new { message = "Donation successful!" });
        }

        // --- ADMIN ENDPOINTS ---
        [HttpGet("admin/users")]
        public IActionResult GetAllUsers() => Ok(_context.Users.Select(u => new { id = u.Id, firstName = u.FirstName, lastName = u.LastName, email = u.Email, role = u.Role }).ToList());

        [HttpGet("admin/donations")]
        public IActionResult GetAllDonations() => Ok(_context.Donations.Include(d => d.User).Include(d => d.Campaign).Select(d => new { id = d.Id, donorName = d.User.FirstName + " " + d.User.LastName, campaignTitle = d.Campaign.Title, amount = d.Amount, date = d.Date.ToString("yyyy-MM-dd"), method = d.Method }).ToList());

        [HttpGet("admin/shifts")]
        public IActionResult GetAllShifts() => Ok(_context.VolunteerShifts.Include(s => s.User).Include(s => s.Campaign).Select(s => new { id = s.Id, volunteerName = s.User.FirstName + " " + s.User.LastName, campaignTitle = s.Campaign.Title, date = s.Date.ToString("yyyy-MM-dd"), time = s.Time, status = s.Status }).ToList());

        [HttpPost("admin/campaign/create")]
        public IActionResult CreateCampaign([FromBody] Campaign newCampaign)
        {
            newCampaign.Status = "Active"; newCampaign.RaisedAmount = 0;
            _context.Campaigns.Add(newCampaign); _context.SaveChanges();
            return Ok(new { message = "Campaign created!" });
        }

        // --- THIS IS THE NEW EDIT ENDPOINT ---
        [HttpPost("admin/campaign/edit")]
        public IActionResult EditCampaign([FromBody] Campaign editCampaign)
        {
            var campaign = _context.Campaigns.Find(editCampaign.Id);
            if (campaign == null) return NotFound();

            campaign.Title = editCampaign.Title;
            campaign.Description = editCampaign.Description;
            campaign.GoalAmount = editCampaign.GoalAmount;

            // Auto-end if the new goal is less than what's already been raised
            if (campaign.RaisedAmount >= campaign.GoalAmount) campaign.Status = "Ended";

            _context.SaveChanges();
            return Ok(new { message = "Campaign updated!" });
        }

        [HttpPost("admin/campaign/delete/{id}")]
        public IActionResult DeleteCampaign(int id)
        {
            var c = _context.Campaigns.Find(id);
            if (c != null) { _context.Campaigns.Remove(c); _context.SaveChanges(); }
            return Ok(new { message = "Campaign deleted." });
        }

        [HttpPost("admin/user/delete/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var u = _context.Users.Find(id);
            if (u != null && u.Role != "Admin") { _context.Users.Remove(u); _context.SaveChanges(); }
            return Ok(new { message = "User deleted." });
        }

        [HttpGet("admin/reports")]
        public IActionResult GetReports()
        {
            var users = _context.Users.ToList();
            var donations = _context.Donations.ToList();
            return Ok(new
            {
                TotalUsers = users.Count,
                TotalDonors = users.Count(u => u.Role == "Donor" || u.Role == "Both"),
                TotalVolunteers = users.Count(u => u.Role == "Volunteer" || u.Role == "Both"),
                TotalDonations = donations.Sum(d => d.Amount),
                AvgDonation = donations.Any() ? donations.Average(d => d.Amount) : 0,
                TotalShifts = _context.VolunteerShifts.Count(),
                ActiveCampaigns = _context.Campaigns.Count(c => c.Status == "Active")
            });
        }
    }

    public class LoginRequest { 
        public string Email { get; set; } 
        public string Password { get; set; } 
    }
    public class RegisterRequest { 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string Email { get; set; } 
        public string Password { get; set; } 
        public string Role { get; set; } 
        public string Phone { get; set; } 
        public string Skills { get; set; } 
    }
    public class UpdateProfileRequest { 
        public int Id { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string Phone { get; set; } 
        public string Skills { get; set; } 
    }
    public class ShiftRequest { 
        public int UserId { get; set; }
        public int CampaignId { get; set; } 
        public DateTime Date { get; set; } 
        public string Time { get; set; } 
    }
    public class CancelShiftRequest { 
        public int UserId { get; set; } 
        public int ShiftId { get; set; } 
    }
    public class DonateRequest { 
        public int UserId { get; set; } 
        public int CampaignId { get; set; } 
        public decimal Amount { get; set; } 
        public string Method { get; set; } 
        public string CardNumber { get; set; } 
    }
}