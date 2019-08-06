using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
//using PoliciesAuthApp1.Services.Requirements;

namespace Jwtauth.Services
{
    public class MinimumExperienceRequirement : IAuthorizationRequirement
    {
        public int MinimumYearsExperience { get; }

        public MinimumExperienceRequirement(int minimumYearsExperience)
        {
            MinimumYearsExperience = minimumYearsExperience;
        }
    }

    public class ExperiencedTraderHandler : AuthorizationHandler<MinimumExperienceRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                    MinimumExperienceRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "jwt.datej"))
            {
                return Task.CompletedTask;
            }

            var dateJoined = Convert.ToDateTime(
                context.User.FindFirst(c => c.Type == "jwt.datej").Value);

            int calculatedExperience = DateTime.Today.Year - dateJoined.Year;
            if (calculatedExperience >= requirement.MinimumYearsExperience)
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
}
