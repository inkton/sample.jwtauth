using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Jwtauth.Helpers;
using Jwtauth.Services;
using Jwtauth.Config;

namespace Jwtauth.Extensions
{
    public static class JwtExtension
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(options => configuration.GetSection("JwtSettings").Bind(options));
                JwtSettings jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                var encryptionKey = Encoding.UTF8.GetBytes(jwtSettings.EncryptKey);

                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ClockSkew = TimeSpan.Zero, // default: 5 min
                    RequireSignedTokens = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidateAudience = true, //default : false
                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuer = true, //default : false
                    ValidIssuer = jwtSettings.Issuer,
                };
                options.Events = new JwtBearerEvents()
                    {                      
                        OnTokenValidated = context =>
                        {
                            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                            if (claimsIdentity.Claims?.Any() != true)
                                context.Fail("This token has no claims.");
                            var claim = claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Exp);
                            var epoch = Convert.ToInt32(claim?.Value);
                            System.Diagnostics.Debug.Print($@"
                                *** Time Now {DateTime.UtcNow}  *** 
                                *** Access Token Expiry {DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime}  *** 
                            ");
                            return Task.CompletedTask;  
                        },
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }                     
                    };
            });

            services.AddSingleton<IJwtFactory, JwtFactory>();
        }
    }
}