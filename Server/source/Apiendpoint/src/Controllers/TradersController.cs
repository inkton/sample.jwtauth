using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Inkton.Nester;
using Inkton.Nester.Queue;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Jwtauth.Database;
using Jwtauth.Model;
using Jwtauth.Services;
using Jwtauth.Helpers;

namespace Jwtauth.Controllers
{
    [ApiController]
    [Route("api/traders")]
    public class TradersController : Controller
    {
        private readonly ILogger _logger;
        private readonly IIndustryRepository _repo;        
        private readonly SignInManager<Trader> _signInManager;
        private readonly UserManager<Trader> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public TradersController(
            ILogger<TradersController> logger,
            IIndustryRepository repo,
            UserManager<Trader> userManager,
            SignInManager<Trader> signInManager,
            IEmailSender emailSender,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _repo = repo;            
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;           
            _configuration = configuration;
        }
        
        // GET api/users/{id}
        [Authorize(Policy = "AllTraders")] 
        [HttpGet("{email}")]
        public async Task<JsonResult> QueryAsync(string id)
        {
            try
            {
                var user = await GetUserAsync(id);

                if (user == null)
                {
                    return this.NestResult(
                        Result.Unauthorized); 
                }
                else
                {
                    return this.NestResultSingle(
                        Result.Succeeded, user);
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
        public async Task<JsonResult> UpdateAsync(string id, 
            [FromQuery] string firstName,
            [FromQuery] string lastName)
        {
            try
            {
                var user = await GetUserAsync(id);

                if (user == null)
                {
                    return this.NestResult(Result.Unauthorized); 
                }

                if (firstName != null)
                {
                    user.FirstName = firstName;
                }
                if (lastName != null)
                {
                    user.LastName = lastName;
                }

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

        private async Task<Trader> GetUserAsync(string id)
        {
            Trader user = await _userManager.GetUserAsync(HttpContext.User);

            if (id != user.Id.ToString())
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var dateJoinedRaw = claims.Where(c => c.Type == "jwt.datej")
                            .Select(c => c.Value).Single();
                if (dateJoinedRaw == null)
                    return null; 
                var dateJoined = DateTime.Parse(dateJoinedRaw);
                if ( (DateTime.Now - dateJoined).TotalDays > (365 * 5))
                {
                    // only experienced traders can change other users
                    return null; 
                }

                user = await _userManager.FindByIdAsync(id);
            }

            return user;
        }
    }
}