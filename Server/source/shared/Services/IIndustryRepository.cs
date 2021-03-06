using System.Threading.Tasks;
using System.Collections.Generic;
using Jwtauth.Model;

namespace Jwtauth.Services
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
