using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Jwtauth.Model;

namespace Jwtauth.Config.Database
{
    public class IndustryConfiguration : IEntityTypeConfiguration<Industry>
    {
        public void Configure(EntityTypeBuilder<Industry> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Tag)
                .IsRequired()
                .HasColumnType("varchar(64)");
            builder.HasIndex(o => o.Tag)
                .IsUnique();

            builder.Property(o => o.Name).HasColumnType("varchar(255)");

            builder.ToTable("Industry");

            builder.HasMany(c => c.Shares)
                .WithOne(e => e.OwnedBy as Industry);

            builder.HasData(
                    new Industry { Id = 1, Tag = "tech", Name = "Technology" },
                    new Industry { Id = 2, Tag = "finc", Name = "Financial" }
                    );
        }
    }    
}