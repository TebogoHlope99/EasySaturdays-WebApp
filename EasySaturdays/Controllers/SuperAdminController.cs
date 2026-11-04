using EasySaturdays.Data;
using EasySaturdays.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EasySaturdays.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly AppDbContext _context;
        public SuperAdminController(AppDbContext context) { _context = context; }

        public IActionResult Index()
        {
            // 1. Load all requests AND include the user who made them
            var requests = _context.AdminRequests
                .Include(r => r.RequestedByUser)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();

            // 2. Load ALL current admins so the Super Admin can see them
            ViewBag.AllAdmins = _context.Users
                .Where(u => u.Role == "Admin" || u.Role == "SuperAdmin")
                .OrderBy(u => u.FirstName)
                .ToList();

            // 3. Pass the stats for the cards
            ViewBag.TotalAdmins = ViewBag.AllAdmins.Count;
            ViewBag.TotalUsers = _context.Users.Count();

            return View(requests);
        }

        [HttpPost]
        public IActionResult ApproveRequest(int requestId)
        {
            var request = _context.AdminRequests.Find(requestId);
            if (request != null && request.Status == "Pending")
            {
                if (request.ActionType == "CreateAdmin")
                {
                    // Check if email already exists
                    if (!_context.Users.Any(u => u.Email.ToLower() == request.NewAdminEmail.ToLower()))
                    {
                        var newAdmin = new User
                        {
                            FirstName = "New",
                            LastName = "Admin",
                            Email = request.NewAdminEmail,
                            Role = "Admin",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
                        };
                        _context.Users.Add(newAdmin);
                    }
                }
                else if (request.ActionType == "DeleteAdmin" && !string.IsNullOrEmpty(request.TargetAdminId))
                {
                    var userToDelete = _context.Users.Find(int.Parse(request.TargetAdminId));
                    if (userToDelete != null && userToDelete.Role != "SuperAdmin")
                    {
                        _context.Users.Remove(userToDelete);
                    }
                }

                request.Status = "Approved";
                _context.SaveChanges();
                TempData["Success"] = "Admin request approved and applied successfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectRequest(int requestId)
        {
            var request = _context.AdminRequests.Find(requestId);
            if (request != null && request.Status == "Pending")
            {
                request.Status = "Rejected";
                _context.SaveChanges();
                TempData["Success"] = "Admin request rejected.";
            }
            return RedirectToAction("Index");
        }
    }
}