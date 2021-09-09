using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class SportsService
    {
        private readonly IConfiguration configuration;
        public SportsService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<SportLeagues> GetSportLeaguesList(int SportCategoryId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<SportLeagues>("Admin_SportLeagues_Get", new { SportCategoryId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<SportTeams> GetSportTeamsList(int SportLeaguesId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<SportTeams>("Admin_SportTeams_Get", new { SportLeaguesId }, commandType: CommandType.StoredProcedure);
        }

    }
}
