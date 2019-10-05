using System.Collections.Generic;
using System.Security.Claims;
using Jwtauth.Model;

namespace Jwtauth.Services
{
    public interface IJwtFactory
    {
        string Create(Trader user, IList<Claim> claims = null);
    }
}
