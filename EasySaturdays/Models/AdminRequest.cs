using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace EasySaturdays.Models
{
    public class AdminRequest
    {
        public int Id { get; set; }

        public int RequestedByUserId { get; set; }
        public User RequestedByUser { get; set; }

        [Required]
        public string ActionType { get; set; } // "CreateAdmin" or "DeleteAdmin"

        public string? TargetAdminId { get; set; } // ID of admin to delete
        public string? NewAdminEmail { get; set; } // Email of new admin to create

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}