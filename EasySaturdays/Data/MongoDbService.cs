using EasySaturdays.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EasySaturdays.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Campaign> Campaigns => _database.GetCollection<Campaign>("Campaigns");
        public IMongoCollection<Donation> Donations => _database.GetCollection<Donation>("Donations");
        public IMongoCollection<VolunteerShift> VolunteerShifts => _database.GetCollection<VolunteerShift>("VolunteerShifts");
        public IMongoCollection<RoleChangeRequest> RoleChangeRequests => _database.GetCollection<RoleChangeRequest>("RoleChangeRequests");
    }
}