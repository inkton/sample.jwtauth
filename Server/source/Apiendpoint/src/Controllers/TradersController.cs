using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Inkton.Nester;
using Jwtauth.Model;
using Jwtauth.Services;
using Jwtauth.Helpers;

namespace Jwtauth.Controllers
{
    [ApiController]
    [Route("api/traders")]
    public class TradersController : JwtauthBaseController
    {
        public TradersController(
            NesterServices nesterServices,
            ILogger<TradersController> logger,
            IIndustryRepository repo,
            UserManager<Trader> userManager)
            : base(nesterServices, logger, repo, userManager)
        { }
        
        // GET api/users/{id}
        [AllowAnonymous]
        [HttpGet("{email}")]
        public async Task<JsonResult> QueryAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return this.NestResult(
                        Result.UserNotFound); 

                if (await IsUserAllowedAsync(user))
                {
                    return this.NestResultSingle(
                        Result.Succeeded, user);
                }
                else
                {
                    // An anonymous query to test whether 
                    // email exits send minimal user information.
                    var minimalUserInfo = new Trader
                    { 
                        Id = user.Id,
                        Email = user.Email
                    };

                    return this.NestResultSingle(
                        Result.Succeeded, minimalUserInfo);
                }
            }
            catch (System.Exception e)
            {
                return this.NestResult(Result.InternalError,
                    e.Message);
            }
        }

        // PUT api/users/{id}
        [Authorize(Policy = "AllTraders")] 
        [HttpPut("{id}")]
        public async Task<JsonResult> UpdateAsync(int id, Trader updated)
        {
            try
            {
                var user = await GetAuthorizedUserAsync(id);

                if (user == null)
                    return this.NestResult(
                        Result.Unauthorized, 
                        "Cannot update user", 401);

                user.FirstName = updated.FirstName;
                user.LastName = updated.LastName;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return this.NestResultSingle(
                        Result.Succeeded, user);
                }
                else
                {
                    return this.NestResult(Result.Failed,
                        JsonConvert.SerializeObject(result.Errors));
                }
            }
            catch (System.Exception e)
            {
                return this.NestResult(Result.InternalError,
                    e.Message);
            }
        }
    }
}