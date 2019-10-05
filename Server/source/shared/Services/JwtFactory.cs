using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Jwtauth.Model;
using Jwtauth.Config;

namespace Jwtauth.Services
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtSettings _jwtOptions;
        private readonly ILogger _logger;

        public JwtFactory(
            IOptions<JwtSettings> jwtOptions,
            ILogger<JwtFactory> logger)
        {
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public string Create(Trader user, IList<Claim> claims = null)
        {
            DateTime now = DateTime.UtcNow;
            DateTime expiry;
            bool isAccessToken = claims != null;

            if (isAccessToken)
            {
                expiry = now.AddMinutes(_jwtOptions.ShortTermExpireMinutes);
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName));
                claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName));
            }
            else
            {
                claims = new List<Claim>();
                expiry = now.AddMinutes(_jwtOptions.LongTermExpireMinutes);
            }

            var baseClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiry).ToUnixTimeMilliseconds().ToString(), ClaimValueTypes.Integer64),
            };

            claims = claims.Concat(baseClaims).ToArray();

            DateTime notBefore = now.AddMinutes(_jwtOptions.NotBeforeMinutes);

            var jwtSecurityToken = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
                issuer : _jwtOptions.Issuer,
                audience : _jwtOptions.Audience,
                issuedAt : now,
                notBefore : notBefore,
                expires : expiry,
                signingCredentials : new SigningCredentials(new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)), 
                        SecurityAlgorithms.HmacSha256Signature),
                subject : new ClaimsIdentity(claims)
            );

            _logger.LogInformation($@"
                *** Token (isAccessToken = {isAccessToken}) *** 
                Time now {now} 
                Not before {notBefore} 
                Expiry {expiry} 
            ");

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}
