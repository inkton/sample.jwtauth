using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using Inkton.Nester;
using Inkton.Nest.Model;
using Jwtauth;

namespace Jwtauth.Database
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {            
            builder.ToTable("User");
        }
    }    
}