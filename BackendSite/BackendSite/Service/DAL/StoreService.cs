using BackendSite.Service.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.DAL
{
    public class StoreService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public StoreService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this._configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }
    }
}