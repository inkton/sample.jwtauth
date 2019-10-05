using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Jwtauth.Model;

namespace Jwtauth.Config.Database
{
    public class TraderConfiguration : IEntityTypeConfiguration<Trader>
    {
        public void Configure(EntityTypeBuilder<Trader> builder)
        {            
            builder.Property(o => o.DateJoined)
                .IsRequired()
                .HasColumnType("datetime");

            builder.ToTable("Trader");
        }
    }
}