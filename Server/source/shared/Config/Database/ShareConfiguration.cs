using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using Inkton.Nester;
using Jwtauth;
using Jwtauth.Model;

namespace Jwtauth.Config.Database
{
    public class ShareConfiguration : IEntityTypeConfiguration<Share>
    {
        public void Configure(EntityTypeBuilder<Share> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Tag)
                .IsRequired()
                .HasColumnType("varchar(64)");
            builder.HasIndex(o => o.Tag)
                .IsUnique();

            builder.Property(o => o.Price).HasColumnType("decimal");

            builder.ToTable("Share");

            builder.HasData(
                new Share { Id = 1, IndustryId = 1, Tag = "BUD", Price = 10.28M },
                new Share { Id = 2, IndustryId = 1, Tag = "LWP", Price = 15.2M },
                new Share { Id = 3, IndustryId = 1, Tag = "WGL", Price = 35.0M },
                new Share { Id = 4, IndustryId = 2, Tag = "MGP", Price = 12.0M },
                new Share { Id = 5, IndustryId = 2, Tag = "MVT", Price = 11.60M },
                new Share { Id = 6, IndustryId = 2, Tag = "GCM", Price = 61.64M }
            );
        }
    }    
}