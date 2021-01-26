using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private readonly SportsBLL sportsBLL;


        public SportsController(SportsBLL sportsBLL)
        {
            this.sportsBLL = sportsBLL;
        }
        public class SportLeaguesReq
        {
            public int SportCategoryId { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<SportLeagues>> GetSportLeaguesList(SportLeaguesReq sportLeaguesReq)
        {
            var apiResult = new ApiResult<IEnumerable<SportLeagues>>();
            apiResult.Data = sportsBLL.GetSportLeaguesList(sportLeaguesReq.SportCategoryId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        public class SportTeamsReq
        {
            public int SportLeaguesId { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<SportTeams>> GetSportTeamsList(SportTeamsReq sportTeamsReq)
        {
            var apiResult = new ApiResult<IEnumerable<SportTeams>>();
            apiResult.Data = sportsBLL.GetSportTeamsList(sportTeamsReq.SportLeaguesId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }

    }
}