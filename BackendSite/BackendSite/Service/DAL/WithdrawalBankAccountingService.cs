using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.WithdrawalBankAccounting;

using Dapper;

using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class WithdrawalBankAccountingService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public WithdrawalBankAccountingService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        #endregion

        #region Methods
        

        public IEnumerable<GetWithdrawalBankAccountingModel> GetAccountingListByTransId(string transId, int siteId, int currencyId)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", transId);
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyId", currencyId);

            return _reportDbClient.Query<GetWithdrawalBankAccountingModel>("Admin_WithdrawalBankAccounting_Get_V3",
                parameter);
        }


        public IEnumerable<WithdrawalAccountingModel> GetByTransId(string transId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TransId", transId);

            return _reportDbClient.Query<WithdrawalAccountingModel>("Admin_WithdrawalBankAccountingByTransId_Get", parameter);
        }

        #endregion
    }
}