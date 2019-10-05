using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Inkton.Nester;
using Jwtauth.Database;
using Jwtauth.Services;
using Jwtauth.Extensions;

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
            services.AddDbContext<JwtauthContext>(options =>
                options.UseSqlite("Data Source=/var/app/source/shared/Jwtauth.db"));
            services.AddScoped<IIndustryRepository, IndustryRepository>();            

            // Set API Version
            services.AddApiVersioning(options => {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1,0);
                options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
                });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo  { Title = "Demo Jwt Auth API", Version = "v1" });
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>(); // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                // or use the generic method, e.g. c.OperationFilter<AppendAuthorizeToSummaryOperationFilter<MyCustomAttribute>>();

                // add Security information to each operation for OAuth2
                options.OperationFilter<SecurityRequirementsOperationFilter>();
                // or use the generic method, e.g. c.OperationFilter<SecurityRequirementsOperationFilter<MyCustomAttribute>>();

                // if you're using the SecurityRequirementsOperationFilter, you also need to tell Swashbuckle you're using OAuth2
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });                
            });

            // Add Identity and JWT (Note sequence important)

            services.AddIdentity(Configuration);
            services.AddJwt(Configuration);
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
