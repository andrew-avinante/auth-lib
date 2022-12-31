using AuthLib.Model.Db.Logger;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthLib.Model.Db
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Log>? Log { get; set; }
        public DbSet<RefreshToken>? RefreshToken { get; set; }
        public DbSet<ApiKeys>? ApiKeys { get; set; }
    }
}
