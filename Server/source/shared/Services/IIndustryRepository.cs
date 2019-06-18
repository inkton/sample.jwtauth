using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Jwtauth.Model;

namespace Jwtauth.Database
{    
    public interface IIndustryRepository
    {        
        List<Industry> ListAllIndustries();

        Task AddShareAsync(Share shares);
        Task UpdateShareAsync(Share shares);
        Task DeleteShareAsync(Share shares);
        Share GetShare(int ShareId);
        List<Share> ListAllShares(int byIndustryId);
    }
}
