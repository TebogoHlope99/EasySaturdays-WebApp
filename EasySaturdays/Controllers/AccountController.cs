using EasySaturdays.Data;
using EasySaturdays.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasySaturdays.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context) { _context = context; }
        
        // ==========================================
        // Profile Actions
        // ==========================================
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            // Check if there is a pending request so the UI can hide the form
            ViewBag.PendingRequest = _context.RoleChangeRequests
                .FirstOrDefault(r => r.UserId == userId && r.Status == "Pending");

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateProfile(User model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Skills = model.Skills;

            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        public IActionResult RequestRoleChange(string requestedRole)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = _context.Users.Find(userId);

            if (user == null || string.IsNullOrEmpty(requestedRole) || requestedRole == user.Role)
            {
                TempData["Error"] = "Invalid role request.";
                return RedirectToAction("Profile");
            }

            var existingRequest = _context.RoleChangeRequests
                .FirstOrDefault(r => r.UserId == userId && r.Status == "Pending");

            if (existingRequest != null)
            {
                TempData["Error"] = "You already have a pending role change request.";
                return RedirectToAction("Profile");
            }

            _context.RoleChangeRequests.Add(new RoleChangeRequest
            {
                UserId = userId,
                CurrentRole = user.Role,
                RequestedRole = requestedRole,
                Status = "Pending"
            });
            _context.SaveChanges();

            TempData["Success"] = $"Request to become a {requestedRole} has been submitted for Admin approval.";
            return RedirectToAction("Profile");
        }

        // ==========================================
        // LOGIN ACTIONS
        // ==========================================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password. Please try again.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(new ClaimsIdentity(claims, "MyCookieAuth")));
            return RedirectToAction("Index", "Dashboard");
        }

        // ==========================================
        // REGISTER ACTIONS
        // ==========================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user, string password, string confirmPassword, string role, bool popia, bool terms)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match. Please try again.";
                return View(user);
            }

            if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsDigit))
            {
                ViewBag.Error = "Password must be at least 8 characters long, contain 1 uppercase letter, and 1 number.";
                return View(user);
            }

            if (!popia || !terms)
            {
                ViewBag.Error = "You must consent to POPIA and the Code of Conduct to register.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName) || string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View(user);
            }

            if (_context.Users.Any(u => u.Email.ToLower() == user.Email.ToLower()))
            {
                ViewBag.Error = "This email address is already registered. Try logging in.";
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Role = role;
            _context.Users.Add(user);
            _context.SaveChanges();

            ViewBag.Success = "Account successfully created! You can now log in.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }
    }
}