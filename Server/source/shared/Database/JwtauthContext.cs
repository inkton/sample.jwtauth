using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Inkton.Nest.Model;
using Inkton.Nester;
using Jwtauth;
using Jwtauth.Model;

namespace Jwtauth.Database
{
    public class JwtauthContext : IdentityDbContext<User, Role, int>
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

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new IndustryConfiguration());
            modelBuilder.ApplyConfiguration(new ShareConfiguration());

            modelBuilder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable("UserRole"); });
            modelBuilder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable("UserClaim"); });
            modelBuilder.Entity<IdentityUserLogin<int>>(entity => { entity.ToTable("UserLogin"); });
            modelBuilder.Entity<IdentityUserToken<int>>(entity => { entity.ToTable("UserToken"); });
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable("RoleClaim"); });
        }
    }

    public static class JwtauthContextFactory
    {
        public static JwtauthContext Create(Runtime runtime)
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
