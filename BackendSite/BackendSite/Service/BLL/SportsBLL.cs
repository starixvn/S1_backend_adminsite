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

        public IEnumerable<SportLeagues> GetSportLeaguesList(int SportCategoryId)
        {
            return sportsService.GetSportLeaguesList(SportCategoryId);
        }
        public IEnumerable<SportTeams> GetSportTeamsList(int SportLeaguesId)
        {
            return sportsService.GetSportTeamsList(SportLeaguesId);
        }

    }
}
