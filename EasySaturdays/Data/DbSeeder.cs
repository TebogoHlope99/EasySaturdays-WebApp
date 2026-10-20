using EasySaturdays.Models;

namespace EasySaturdays.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // ==========================================
            // 1. SEED USERS (Admin, Volunteer, Donor, Both)
            // ==========================================
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        FirstName = "System",
                        LastName = "Admin",
                        Email = "admin@easysaturdays.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role = "Admin"
                    },
                    new User
                    {
                        FirstName = "Thabo",
                        LastName = "Volunteer",
                        Email = "volunteer@demo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Volunteer"
                    },
                    new User
                    {
                        FirstName = "Lerato",
                        LastName = "Donor",
                        Email = "donor@demo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Donor"
                    },
                    new User
                    {
                        FirstName = "Sipho",
                        LastName = "Both",
                        Email = "both@demo.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                        Role = "Both"
                    }
                );
                context.SaveChanges(); // Save so we can get the User IDs
            }

            // ==========================================
            // 2. SEED CAMPAIGNS (3 Campaigns)
            // ==========================================
            if (!context.Campaigns.Any())
            {
                context.Campaigns.AddRange(
                    new Campaign
                    {
                        Title = "Back-2-School 2026",
                        Description = "Help us provide school shoes and uniforms to 500+ children in Tembisa.",
                        GoalAmount = 50000,
                        RaisedAmount = 0
                    },
                    new Campaign
                    {
                        Title = "Winter Warmth",
                        Description = "Ensure our senior citizens and vulnerable families have warm food and blankets.",
                        GoalAmount = 30000,
                        RaisedAmount = 0
                    },
                    new Campaign
                    {
                        Title = "Community Food Drive",
                        Description = "Stock the local food bank to support 200+ families facing food insecurity.",
                        GoalAmount = 20000,
                        RaisedAmount = 0
                    }
                );
                context.SaveChanges(); // Save so we can get the Campaign IDs
            }

            // ==========================================
            // 3. SEED DONATIONS (5 Donations)
            // ==========================================
            if (!context.Donations.Any())
            {
                // Fetch the seeded data to link relationships correctly
                var donor = context.Users.First(u => u.Email == "donor@demo.com");
                var bothUser = context.Users.First(u => u.Email == "both@demo.com");

                var schoolCampaign = context.Campaigns.First(c => c.Title == "Back-2-School 2026");
                var winterCampaign = context.Campaigns.First(c => c.Title == "Winter Warmth");
                var foodCampaign = context.Campaigns.First(c => c.Title == "Community Food Drive");

                var donations = new List<Donation>
                {
                    new Donation { Amount = 500, Date = DateTime.Now.AddDays(-10), UserId = donor.Id, CampaignId = schoolCampaign.Id, Method = "Credit Card" },
                    new Donation { Amount = 250, Date = DateTime.Now.AddDays(-18), UserId = donor.Id, CampaignId = winterCampaign.Id, Method = "Instant EFT" },
                    new Donation { Amount = 1000, Date = DateTime.Now.AddDays(-8), UserId = bothUser.Id, CampaignId = foodCampaign.Id, Method = "Mobile Money" },
                    new Donation { Amount = 150, Date = DateTime.Now.AddDays(-5), UserId = bothUser.Id, CampaignId = schoolCampaign.Id, Method = "Cash" },
                    new Donation { Amount = 750, Date = DateTime.Now.AddDays(-2), UserId = donor.Id, CampaignId = foodCampaign.Id, Method = "Credit Card" }
                };

                context.Donations.AddRange(donations);
                context.SaveChanges();

                // ==========================================
                // 4. UPDATE CAMPAIGN RAISED AMOUNTS
                // ==========================================
                // Automatically calculate the RaisedAmount based on the donations we just added
                schoolCampaign.RaisedAmount = donations.Where(d => d.CampaignId == schoolCampaign.Id).Sum(d => d.Amount);
                winterCampaign.RaisedAmount = donations.Where(d => d.CampaignId == winterCampaign.Id).Sum(d => d.Amount);
                foodCampaign.RaisedAmount = donations.Where(d => d.CampaignId == foodCampaign.Id).Sum(d => d.Amount);

                context.SaveChanges();
            }
        }
    }
}