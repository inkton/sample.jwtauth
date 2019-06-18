using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Inkton.Nest.Model;
using Inkton.Nester;
using Jwtauth.Database;
using Jwtauth.Model;
using Jwtauth.Services;

namespace Jwtauth
{   
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNester();

            // 1. Set API Version

            services.AddDbContext<JwtauthContext>(options =>
                options.UseSqlite("Data Source=/var/app/source/shared/Jwtauth.db"));
            services.AddScoped<IIndustryRepository, IndustryRepository>();
            services.AddApiVersioning(options => {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1,0);
                options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
                });
            services.AddSwaggerGen(options => {
               options.SwaggerDoc("v1", new Info { Title = "Demo Jwt Auth API", Version = "v1" });
            });

            // 2. Configure JWT Authentication

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidIssuer = Configuration["JwtIssuer"],
                    ValidAudience = Configuration["JwtIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                    ClockSkew = TimeSpan.Zero // remove delay of token when expire
                };
            });

            // 3. Configure .Net Core Identity

            services.AddIdentity<User, Role>(options => {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
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

            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(options => {
                options.AddPolicy("AllAllowed", policy => policy.RequireRole("User", "Admin"));
                options.AddPolicy("OnlyAdminsAllowed", policy => policy.RequireRole("Admin"));
            });

            // 4. Configure Email for sending out security codes

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGrid"));
        }

        // This method gets called by the runtime. 
        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseSwagger();

            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("v1/swagger.json", "Demo Jwt Auth API V1");
            });

            app.UseMvc();
            app.Run(context => {
                context.Response.Redirect("swagger/");
                return Task.CompletedTask;
            });

            DBinitialize.EnsureCreated(app.ApplicationServices);
        }
    }
}
