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

namespace Jwtauth.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/permits")]
    public class PermitsController : Controller
    {
        private readonly ILogger _logger;
        private readonly IIndustryRepository _repo;        
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public PermitsController(
            ILogger<PermitsController> logger,
            IIndustryRepository repo,
            Runtime runtime,            
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _repo = repo;            
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;           
            _configuration = configuration;
        }
        
        [HttpPost]
        public async Task<object> RegisterAsync(Permit permit)
        {
            try
            {
                // The user is registered in 2 steps. The fist
                // step is when the email and password is provided.
                // An email is sent to the email address to confirm.
                // The user then completes the registration with 
                // the security code sent in the email.

                if (String.IsNullOrEmpty(permit.SecurityCode))
                {
                    // Step 1 : user onboarding
                    return await CreateUserAsync(permit);
                }
                else
                {
                    // Step 2 : confirm the user details
                    return await ConfirmUserAsync(permit);
                }
            }
            catch (System.Exception e)
            {
                return this.NestResult(Result.InternalError,
                    e.Message);
            }
        }

        // GET api/permits/{email}
        [HttpGet("{email}")]
        public async Task<object> QueryTokenAsync(string email, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                return this.NestResult(Result.IncorrectEmail); 
            }
            
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            
            if (result.Succeeded)
            {
                Permit permit = new Permit();
                permit.Password = password;
                permit.Owner = await _userManager.FindByNameAsync(email);
                permit.Token = await GenerateJwtTokenAsync(permit.Owner);

                return this.NestResultSingle(
                    Result.Succeeded, permit, "A new token has been created for the permit");
            }

            return this.NestResult(Result.LoginFailed);
        }

        // PUT api/permits/{email}
        [HttpPut("{email}")]
        public async Task<object> ResetPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return this.NestResult(Result.IncorrectEmail); 
            }
            
            var user = await _userManager.FindByNameAsync(email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return this.NestResult(Result.Failed); 
            }

            string password = Guid.NewGuid().ToString("N").ToLower()
                      .Replace("1", "").Replace("o", "").Replace("0","")
                      .Substring(0,10);

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return this.NestResult(Result.Failed,
                    JsonConvert.SerializeObject(result.Errors));                                  
            }
            
            await _emailSender.SendEmailAsync(email, "Password Reset",
                $"Your password was changed to {password}.");

            return this.NestResultSingle(
                Result.Succeeded, user, "An email was sent to " + email + 
                " with a new password. Login and change the password.");                                        
        }
  
        private async Task<object> CreateUserAsync(Permit permit)
        {
            // create user and confirm Pemail via a security code
            var result = await _userManager.CreateAsync(permit.Owner, permit.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(permit.Owner);
                
                await _emailSender.SendEmailAsync(permit.Owner.Email, "Confirm your email",
                    $"Co    mplete the registration using the security code {code}.");

                return this.NestResultSingle(
                    Result.Succeeded, permit, "An email was sent to " + permit.Owner.Email + 
                    " with a security code. Register user details with the security code.");                                        
            }
            else
            {
                return this.NestResult(Result.Failed,
                    JsonConvert.SerializeObject(result.Errors));                                  
            }
        }

        private async Task<object> ConfirmUserAsync(Permit permit)
        {
            if (permit.Owner.Id == 0)
            {
                var user = await _userManager.FindByNameAsync(permit.Owner.Email);
                user.CopyTo(permit.Owner);
            }

            var result = await _userManager.ConfirmEmailAsync(
                permit.Owner, permit.SecurityCode);

            if (result.Succeeded)
            {
                await _userManager.UpdateAsync(permit.Owner);
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

                if (adminUsers.Count == 0)
                {
                    // Make the first user into the Admin   
                    await _userManager.AddToRoleAsync(permit.Owner, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(permit.Owner, "User");
                }

                // Update the user and return                        
                return this.NestResultSingle(
                    Result.Succeeded, permit, 
                    "This user is now officially a member"); 
            }
            else
            {
                return this.NestResult(Result.IncorrectSecurityCode, 
                    JsonConvert.SerializeObject(result.Errors));
            }
        }

        async private Task<string> GenerateJwtTokenAsync(User user)
        {
            IdentityOptions options = new IdentityOptions();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString(), ClaimValueTypes.Integer64)
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if(role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach(Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(Convert.ToDouble(_configuration["JwtExpireHours"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }        
    }
}