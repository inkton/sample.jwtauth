using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Jwtauth.Database;
using Jwtauth.Model;

namespace Jwtauth.Services
{
    public class IndustryRepository : IIndustryRepository
    {
        private readonly JwtauthContext _dbContext;

        public IndustryRepository (JwtauthContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Industry> ListAllIndustries()
        {
            return _dbContext.Industries.ToList();
        }

        public async Task AddShareAsync(Share shares)
        {
            _dbContext.Add(shares);
            await _dbContext.SaveChangesAsync();
        }        

        public async Task UpdateShareAsync(Share shares)
        {
            _dbContext.Update(shares);
            await _dbContext.SaveChangesAsync();
        }        

        public async Task DeleteShareAsync(Share shares)
        {
            _dbContext.Remove(shares);
            await _dbContext.SaveChangesAsync();
        }

        public Share GetShare(int ShareId)
        {
            var existingRec = _dbContext.Shares
                .FirstOrDefault(o => o.Id == ShareId);
            return existingRec;
        }

        public List<Share> ListAllShares(int byIndustryId)
        {
            var existingRecs = _dbContext.Shares
                .Where(o => o.IndustryId == byIndustryId);
            return existingRecs.ToList();
        }
    }
}
