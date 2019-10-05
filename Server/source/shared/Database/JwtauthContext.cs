using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Jwtauth.Config.Database;
using Jwtauth.Model;

namespace Jwtauth.Database
{
    public class JwtauthContext : IdentityDbContext<Trader, IdentityRole<int>, int>
    {
        public JwtauthContext (DbContextOptions<JwtauthContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Industry> Industries { get; set; }
        public DbSet<Share> Shares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TraderConfiguration());
            modelBuilder.ApplyConfiguration(new IndustryConfiguration());
            modelBuilder.ApplyConfiguration(new ShareConfiguration());

            modelBuilder.Entity<IdentityRole<int>>(entity => { entity.ToTable("Role"); });
            modelBuilder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable("UserRole"); });
            modelBuilder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable("UserClaim"); });
            modelBuilder.Entity<IdentityUserLogin<int>>(entity => { entity.ToTable("UserLogin"); });
            modelBuilder.Entity<IdentityUserToken<int>>(entity => { entity.ToTable("UserToken"); });
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable("RoleClaim"); });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }        
    }

    public static class JwtauthContextFactory
    {
        public static JwtauthContext Create()
        {
            var optionsBuilder = new DbContextOptionsBuilder<JwtauthContext>();

            string connectionString = "Data Source=/var/app/source/shared/Jwtauth.db";

            optionsBuilder.UseSqlite(connectionString);
            var context = new JwtauthContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
