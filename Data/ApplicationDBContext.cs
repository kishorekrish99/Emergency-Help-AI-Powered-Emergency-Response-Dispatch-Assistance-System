using EmergencyHelp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHelp.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }

        public DbSet<Emergency> Emergencies { get; set; }
    }
}
