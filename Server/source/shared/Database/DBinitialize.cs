using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jwtauth.Database
{
    public static class DBinitialize
    {
        public static void EnsureCreated(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                serviceScope.ServiceProvider.GetService<JwtauthContext>().Database.EnsureCreated();
            }
        }
    }
}