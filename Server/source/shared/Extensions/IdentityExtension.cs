using System;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jwtauth.Database;
using Jwtauth.Model;
using Jwtauth.Helpers;
using Jwtauth.Services;
using Jwtauth.Config;

namespace Jwtauth.Extensions
{
    public static class IdentityExtension
    {
        public static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentitySettings>(options => configuration.GetSection("IdentitySettings").Bind(options));
            IdentitySettings identitySettings = configuration.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();            

            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddIdentity<Trader, IdentityRole<int>>(options => {
                // Password settings.
                options.Password.RequireDigit = identitySettings.PasswordRequireDigit;
                options.Password.RequireLowercase = identitySettings.PasswordRequireLowercase;
                options.Password.RequireNonAlphanumeric = identitySettings.PasswordRequireNonAlphanumic;
                options.Password.RequireUppercase = identitySettings.PasswordRequireUppercase;
                options.Password.RequiredLength = identitySettings.PasswordRequiredLength;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Set emailed token for both
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;                
            })
            .AddEntityFrameworkStores<JwtauthContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AllTraders", builder => builder.RequireAuthenticatedUser().Build());
                options.AddPolicy("SkilledTraders", policy =>
                    policy.Requirements.Add(new MinimumExperienceRequirement(1)));
                options.AddPolicy("TraderManagers", policy =>
                    policy.Requirements.Add(new MinimumExperienceRequirement(5)));
            });

            services.AddSingleton<IAuthorizationHandler, ExperiencedTraderHandler>();

            services.Configure<SendGridSettings>(options => configuration.GetSection("SendGridSettings").Bind(options));
            services.AddTransient<IEmailSender, EmailSender>();
        }
    }
}