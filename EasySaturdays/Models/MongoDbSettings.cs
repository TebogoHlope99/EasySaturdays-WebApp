using Microsoft.AspNetCore.Mvc;

namespace EasySaturdays.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}
