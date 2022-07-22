using System;
using BackendSite.Service.Model;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class SportsService
    {
        private readonly MySqlClient _mainDbClient;
        
        public SportsService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public IEnumerable<SportLeagues> GetSportLeaguesList(int sportCategoryId)
        {
            return _mainDbClient.Query<SportLeagues>("Admin_SportLeagues_Get", new DynamicParameters(new {SportCategoryId = sportCategoryId }));
        }
        public IEnumerable<SportTeams> GetSportTeamsList(int sportLeaguesId)
        {
            return _mainDbClient.Query<SportTeams>("Admin_SportTeams_Get", new DynamicParameters(new {SportLeaguesId = sportLeaguesId }));
        }

    }
}
