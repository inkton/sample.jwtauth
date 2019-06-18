using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using Inkton.Nester;
using Inkton.Nest.Model;
using Jwtauth;

namespace Jwtauth.Database
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(o => o.Tag)
                .IsRequired()
                .HasColumnType("varchar(64)");
            builder.HasIndex(o => o.Tag)
                .IsUnique();

            builder.ToTable("Role");

            builder.HasData(
                new Role { Id = 1, Tag = "user", Name = "User", NormalizedName = "USER" },
                new Role { Id = 2, Tag = "admin", Name = "Admin", NormalizedName = "ADMIN" }
            );
        }
    }    
}