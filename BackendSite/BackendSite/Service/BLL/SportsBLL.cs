using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class SportsBLL
    {
        private readonly SportsService sportsService;


        public SportsBLL(SportsService sportsService)
        {
            this.sportsService = sportsService;
        }

        public IEnumerable<SportLeagues> GetSportLeaguesList(int sportCategoryId)
        {
            return sportsService.GetSportLeaguesList(sportCategoryId);
        }
        public IEnumerable<SportTeams> GetSportTeamsList(int sportLeaguesId)
        {
            return sportsService.GetSportTeamsList(sportLeaguesId);
        }

    }
}
