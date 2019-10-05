using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Inkton.Nester;
using Inkton.Nest.Model;
using Jwtauth.Model;
using Jwtauth.Services;
using Jwtauth.Helpers;

namespace Jwtauth.Controllers
{
    [ApiController]
    [Route("api/permits")]
    public class PermitsController : JwtauthBaseController
    {
        private readonly SignInManager<Trader> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IJwtFactory _jwtFactory;

        public PermitsController(
            NesterServices nesterServices,
            ILogger<PermitsController> logger,
            IIndustryRepository repo,
            UserManager<Trader> userManager,          
            SignInManager<Trader> signInManager,
            IEmailSender emailSender,
            IJwtFactory jwtFactory) 
            : base(nesterServices, logger, repo, userManager)
        {
            _signInManager = signInManager;
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

                var existingUser = await _userManager.FindByNameAsync(permit.User.Email);

                // If the email still hasn't been confirmed - request another
                if (existingUser != null && existingUser.AccessFailedCount < MaxFailedCount
                    && !await _userManager.IsEmailConfirmedAsync(existingUser))
                {
                    await RequestEmailConfirmationAsync(existingUser);

                    await _userManager.AccessFailedAsync(existingUser);

                    return this.NestResultSingle(
                        Result.Succeeded, permit);
                }

                var result = await _userManager.CreateAsync(permit.User, password);

                if (result.Succeeded)
                {
                    await _userManager.AddClaimAsync(permit.User,
                        new Claim(ClaimNames.Trader, permit.User.DateJoined.ToString()));

                    _logger.LogInformation("User created a new account with password.");

                    await RequestEmailConfirmationAsync(permit.User);

                    // Update the user and return 
                    // --------------------------
                    // NOTE: The user object in the permit does have a password hash that should 
                    // not be exposed to the untrusted client. The framework makes sure the 
                    // hash is not copied to the client.

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
            // Here 'AllTraders' are allowed access here. 
            // Although an ordinary trader should not
            // be able to renew access of another trader
            // using their email address. GetAuthorizedUserAsync 
            // returns the operating user only if the user has
            // the authority to impersonate another trader.

            var user = await GetAuthorizedUserAsync(email);

            if (user == null)
                return this.NestResult(
                    Result.Unauthorized, 
                    "Cannot update user", 401);


            var renewedPermit = new Permit<Trader>();
            // --------------------------
            // NOTE: The user object in the permit does have a password hash that should 
            // not be exposed to the untrusted client. The framework makes sure the 
            // hash is not copied to the client.
            renewedPermit.User = user;

            var savedToken = await _userManager.GetAuthenticationTokenAsync(
                user, "JWTSample", "RefreshToken");

            var receivedToken = await HttpContext.GetTokenAsync(
                JwtBearerDefaults.AuthenticationScheme, "access_token");

            if (savedToken != receivedToken)
                return this.NestResult(Result.Failed); 

            renewedPermit.RefreshToken = savedToken;
            renewedPermit.AccessToken = _jwtFactory.Create(user,
                    await _userManager.GetClaimsAsync(user));
            
            return this.NestResultSingle(
                Result.Succeeded, renewedPermit);
        }

        // DELETE api/permits/{email}
        [Authorize(Policy = "AllTraders")] 
        [HttpDelete("{email}")]
        public async Task<JsonResult> RevokeAccessAsync(string email,
            Permit<Trader> permit)
        {            
            var user = await GetAuthorizedUserAsync(email);

            if (user == null)
                return this.NestResult(
                    Result.Unauthorized, 
                    "Cannot update user", 401);

            await _userManager.RemoveAuthenticationTokenAsync(
                user, "JWTSample", "RefreshToken");
            
            permit.RefreshToken = null;
            permit.AccessToken = null;

            return this.NestResultSingle(
                Result.Succeeded, permit);
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
                // --------------------------
                // NOTE: The user object in the permit does have a password hash that should 
                // not be exposed to the untrusted client. The framework makes sure the 
                // hash is not copied to the client.
                permit.User = user;
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
    }
}