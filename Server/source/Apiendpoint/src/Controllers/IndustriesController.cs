using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Inkton.Nester;
using Jwtauth.Model;
using Jwtauth.Services;
using Jwtauth.Helpers;

namespace Jwtauth.Controllers
{
    [ApiController]
    [Route("api/industries")]
    public class IndustriesController : JwtauthBaseController
    {
        public IndustriesController(
            NesterServices nesterServices,
            ILogger<IndustriesController> logger,
            IIndustryRepository repo,
            UserManager<Trader> userManager)
            : base(nesterServices, logger, repo, userManager)
        { }

        [HttpGet]
        [Authorize(Policy = "SkilledTraders")] 
        public JsonResult Get()
        {
            try
            {
                return this.NestResultMultiple(
                     Result.Succeeded,
                    _repo.ListAllIndustries());
            }
            catch (System.Exception e)
            {
                return this.NestResult(
                    Result.InternalError,
                    e.Message);
            }
        }

        // GET api/industries/{id}/shares
        [Authorize(Policy = "SkilledTraders")] 
        [HttpGet("{industry_id}/shares")]        
        public JsonResult QueryShares(int industry_id)
        {
            try
            {
                return this.NestResultMultiple(
                    Result.Succeeded, 
                    _repo.ListAllShares(industry_id));
            }
            catch (System.Exception e)
            {
                return this.NestResult(
                    Result.InternalError,
                    e.Message);
            }
        }

        // GET api/industries/:{id}/shares/{share_id}
        [Authorize(Policy = "SkilledTraders")] 
        [HttpGet("{industry_id}/shares/{share_id}")]
        public JsonResult QueryShare(int industry_id, int share_id)
        {
            try
            {
                var share = _repo.GetShare(share_id);
                if (share != null && share.IndustryId == industry_id)
                {
                    // simulate market price fluctuation                    
                    Random random = new Random();  
                    int volatility = random.Next(-10, 10);  
                    share.Price += volatility;

                    return this.NestResultSingle(
                        Result.Succeeded, share); 
                }
                else
                {
                    return this.NestResult(
                        Result.InvalidShare);                     
                }
            }
            catch (System.Exception e)
            {
                return this.NestResult(
                    Result.InternalError,
                    e.Message);
            }
        } 

        [HttpPost("{industry_id}/shares")]
        [Authorize(Policy = "TraderManagers")] 
        public JsonResult Create(int industry_id, [FromBody] Share share)
        {
            try
            {
                if (share.IndustryId == industry_id)
                {                
                    _repo.AddShareAsync(share);
                    return this.NestResultSingle(
                        Result.Succeeded, share);
                }
                else
                {
                    return this.NestResult(
                        Result.InvalidShare);                     
                }
            }
            catch (System.Exception e)
            {
                 return this.NestResult(
                    Result.InternalError,
                    e.Message);
            }
        }

        [HttpPut("{industry_id}/shares/{share_id}")]
        [Authorize(Policy = "TraderManagers")] 
        public JsonResult Update(int industry_id, int share_id, [FromBody] Share share)
        {
            try
            {
                if (share.IndustryId == industry_id)
                {                
                    _repo.UpdateShareAsync(share);
                    return this.NestResultSingle(
                        Result.Succeeded, share);
                }
                else
                {
                    return this.NestResult(
                        Result.InvalidShare);                     
                }
            }
            catch (System.Exception e)
            {
                return this.NestResult(
                    Result.InternalError,
                    e.Message);
            }
        }

        [HttpDelete("{industry_id}/shares/{share_id}")]
        [Authorize(Policy = "TraderManagers")] 
        public JsonResult Delete(int industry_id, int share_id, [FromBody] Share share)
        {
            try
            {
                _repo.DeleteShareAsync(share);

                return this.NestResultSingle(
                    Result.Succeeded, share);
            }
            catch (System.Exception e)
            {
                return this.NestResult(
                    Result.InternalError,
                    e.Message);
            }
        }
    }
}
