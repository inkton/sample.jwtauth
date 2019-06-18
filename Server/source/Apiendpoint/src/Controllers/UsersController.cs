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

namespace Jwtauth.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly ILogger _logger;
        private readonly IIndustryRepository _repo;        
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public UsersController(
            ILogger<UsersController> logger,
            IIndustryRepository repo,
            Runtime runtime,            
            UserManager<User> userManager,
            SignInManager<User> signInManager,
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
        [HttpGet("{email}")]
        public async Task<object> QueryAsync(string id)
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
        [HttpPut("{id}")]
        public async Task<object> UpdateAsync(string id, [FromQuery] string password)
        {
            try
            {
                var user = await GetUserAsync(id);

                if (user == null)
                {
                    return this.NestResult(Result.Unauthorized); 
                }

                if (password != null)
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
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

        [HttpPost("{id}/roles/{role}")]
        public async Task<object> AddUserRoleAsync(string id, string role)
        {
            try
            {
                var user = await GetUserAsync(id);

                if (user == null)
                {
                    return this.NestResult(Result.Unauthorized); 
                }

                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
 
                    if (!result.Succeeded)
                    {
                        return this.NestResult(Result.Failed,
                            JsonConvert.SerializeObject(result.Errors));
                    }
                }

                return this.NestResult(Result.Succeeded);
            }
            catch (System.Exception e)
            {
                return this.NestResult(Result.InternalError,
                    e.Message);
            }
        }

        [HttpDelete("{id}/roles/{role}")]
        public async Task<object> RemoveUserRoleAsync(string id, string role)
        {
            try
            {
                var user = await GetUserAsync(id);

                if (user == null)
                {
                    return this.NestResult(Result.Unauthorized); 
                }

                if (await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role);
 
                    if (!result.Succeeded)
                    {
                        return this.NestResult(Result.Failed,
                            JsonConvert.SerializeObject(result.Errors));
                    }
                }

                return this.NestResult(Result.Succeeded);
            }
            catch (System.Exception e)
            {
                return this.NestResult(Result.InternalError,
                    e.Message);
            }
        }

        private async Task<User> GetUserAsync(string id)
        {
            User user = await _userManager.GetUserAsync(HttpContext.User);

            if (id != user.Id.ToString())
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    // only admin can change other users
                    return null; 
                }

                user = await _userManager.FindByIdAsync(id);
            }

            return user;
        }
    }
}