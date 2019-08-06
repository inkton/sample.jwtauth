using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
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
    [Route("api/permits")]
    public class PermitsController : Controller
    {
        private readonly ILogger _logger;
        private readonly IIndustryRepository _repo;
        private readonly SignInManager<Trader> _signInManager;
        private readonly UserManager<Trader> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IJwtFactory _jwtFactory;

        public PermitsController(
            ILogger<PermitsController> logger,
            IIndustryRepository repo,            
            SignInManager<Trader> signInManager,
            UserManager<Trader> userManager,
            IEmailSender emailSender,
            IJwtFactory jwtFactory
            )
        {
            _logger = logger;
            _repo = repo;           
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _jwtFactory = jwtFactory;  
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> RegisterAsync(Permit<Trader> permit, [FromQuery] string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permit.User.Email))
                    return this.NestResult(Result.IncorrectEmail);
                if (string.IsNullOrWhiteSpace(password))
                    return this.NestResult(Result.PasswordRequired);

                // create user and confirm email via a security code
                Trader newTrader = new Trader();
                newTrader.FirstName = permit.User.FirstName;
                newTrader.LastName = permit.User.LastName;
                newTrader.Email = permit.User.Email;
                newTrader.UserName = permit.User.UserName;
                newTrader.DateJoined = permit.User.DateJoined;

                var result = await _userManager.CreateAsync(newTrader, password);

                if (result.Succeeded)
                {
                    await _userManager.AddClaimAsync(newTrader,
                        new Claim("jwt.datej", newTrader.DateJoined.ToString()));

                    _logger.LogInformation("User created a new account with password.");

                    await RequestEmailConfirmationAsync(newTrader);

                    // Copy the database assigned id
                    permit.User.Id = newTrader.Id;

                    // Update the user and return                        
                    return this.NestResultSingle(
                        Result.Succeeded, permit); 
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

        // PUT api/permits/{email}
        [AllowAnonymous]
        [HttpPut("{email}")]
        public async Task<JsonResult> SetupPermitAsync(string email,
            Permit<Trader> permit,
            [FromQuery] string action, 
            [FromQuery] string securityCode,
            [FromQuery] string password            
            )
        {
            if (string.IsNullOrEmpty(email))
                return this.NestResult(Result.IncorrectEmail); 
            if (string.IsNullOrEmpty(action))
                return this.NestResult(Result.Failed); 

            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                // Don't reveal that the user does not exist or is not confirmed
                return this.NestResult(Result.UserNotFound); 

            switch (action)
            {
            case "RequestEmailConfirmation":
                // for -> ConfirmEmail
                // for -> ChangePassword
                await RequestEmailConfirmationAsync(user);
                permit.User.Id = 0;
                permit.User.Email = email;
                return this.NestResultSingle(
                    Result.Succeeded, permit);

            case "ConfirmEmail":
                if (await IsEmailConfirmedAsync(user, securityCode))
                {
                    return await CreatePermitAsync(user, password, permit);
                }
                else
                {
                    return this.NestResult(Result.IncorrectSecurityCode); 
                }

            case "Login": 
                return await CreatePermitAsync(user, password, permit);

            case "ChangePassword":
                if (await IsEmailConfirmedAsync(user, securityCode))
                {
                    if (string.IsNullOrEmpty(password))
                        return this.NestResult(Result.PasswordRequired); 

                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);    
                    await _userManager.UpdateAsync(user);
                    await _emailSender.SendEmailAsync(email, "Password Reset",
                        "Your password was changed. Check if this is not you.");

                    return await CreatePermitAsync(user, password, permit);
                }
                else
                {
                    return this.NestResult(Result.IncorrectSecurityCode); 
                }
            }

            return this.NestResult(Result.Failed); 
        }

        // GET api/permits/{email}
        [Authorize(Policy = "AllTraders")] 
        [HttpGet("{email}")]
        public async Task<JsonResult> RenewAccessAsync(string email)
        {
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            var user = await _userManager.FindByNameAsync(email);

            // A valid user emailed confirmed user must exist
            if (user == null || email != user.Email || !(await _userManager.IsEmailConfirmedAsync(user)))
                return this.NestResult(Result.Failed); 

            var savedToken = await _userManager.GetAuthenticationTokenAsync(
                user, "JWTSample", "RefreshToken");

            var receivedToken = await HttpContext.GetTokenAsync(
                JwtBearerDefaults.AuthenticationScheme, "access_token");

            if (savedToken != receivedToken)
                return this.NestResult(Result.Failed); 

            var renewedPermit = new Permit<Trader>();
            SafeCopy(user, renewedPermit.User);
            renewedPermit.RefreshToken = savedToken;
            renewedPermit.AccessToken = _jwtFactory.Create(user,
                    await _userManager.GetClaimsAsync(user));
            
            return this.NestResultSingle(
                Result.Succeeded, renewedPermit);
        }

        // DELETE api/permits/{email}
        [Authorize(Policy = "TraderManagers")] 
        [HttpDelete("{email}")]
        public async Task<JsonResult> RevokeAccessAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return this.NestResult(Result.IncorrectEmail); 

            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return this.NestResult(Result.UserNotFound); 

            await _userManager.RemoveAuthenticationTokenAsync(user, "JWTSample", "RefreshToken");
            
            return this.NestResult(Result.Succeeded);
        }

        async private Task<JsonResult> CreatePermitAsync(Trader user, string password,
            Permit<Trader> permit)
        {
            if (string.IsNullOrEmpty(password))
                return this.NestResult(Result.PasswordRequired); 

            Permit<Trader> newPermit = await LoginAsync(user, password, permit);
            if (newPermit != null)
                return this.NestResultSingle(
                    Result.Succeeded, newPermit);
            else
                return this.NestResult(Result.LoginFailed); 
        }

        async private Task<Permit<Trader>> LoginAsync(Trader user, string password,
            Permit<Trader> permit)
        {
            var result = await _signInManager.PasswordSignInAsync(
                user.Email, password, false, false);
            
            if (result.Succeeded)
            {
                SafeCopy(user, permit.User);
                return await CreateTokensAsync(user, permit);
            }
            else
            {
                return null;
            }
        }

        async private Task<Permit<Trader>> CreateTokensAsync(Trader user, Permit<Trader> permit)
        {
            permit.RefreshToken = _jwtFactory.Create(user);
            permit.AccessToken = _jwtFactory.Create(user,
                    await _userManager.GetClaimsAsync(user));

            await _userManager.RemoveAuthenticationTokenAsync(user, "JWTSample", "RefreshToken");
            await _userManager.SetAuthenticationTokenAsync(user, "JWTSample", "RefreshToken", permit.RefreshToken);

            await _signInManager.SignInAsync(user, true);

            return permit;
        }

        async private Task RequestEmailConfirmationAsync(Trader user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Complete the registration using the security code {code}.");
        }

        async private Task<bool> IsEmailConfirmedAsync(Trader user, string securityCode)
        {
            if (string.IsNullOrEmpty(securityCode))
            {
                return false; 
            }

            var result = await _userManager.ConfirmEmailAsync(
                user, securityCode);

            return (result.Succeeded);
        }

        private void SafeCopy(Trader from, Trader to)
        {
            // Send back the non-security
            // sensitive properties 
            to.Id = from.Id;
            to.Email = from.Email;
            to.FirstName = from.FirstName;
            to.LastName = from.LastName;
            to.DateJoined = from.DateJoined;
        }
    }
}