using BackendSite.Service.Library;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

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

        public IEnumerable<WithdrawalInfoAndAccountingListModel> GetWithdrawalInfoAndAccountingList(string siteIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            string transId,
            string userName,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_SiteIds", siteIds);
            parameter.Add("_CurrencyIds", currencyIds);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_TransId", transId);
            parameter.Add("_UserName", userName);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = _reportDbClient.Query<WithdrawalInfoAndAccountingListModel>(
                "Admin_WithdrawalInfoAndAccounting_Get_V3", parameter);


            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }

        public IEnumerable<WithdrawalInfoAndAccountingListForExportModel> GetWithdrawalInfoAndAccountingListForExport(
            string siteIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            string transId,
            string userName)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_SiteIds", siteIds);
            parameter.Add("_CurrencyIds", currencyIds);
            parameter.Add("_StartDate", startDate);
            parameter.Add("_EndDate", endDate);
            parameter.Add("_TransId", transId);
            parameter.Add("_UserName", userName);

            return _reportDbClient.Query<WithdrawalInfoAndAccountingListForExportModel>(
                "Admin_WithdrawalInfoAndAccountingForExport_Get_V3", parameter);
        }

        public IEnumerable<GetWithdrawalBankAccountingModel> GetAccountingListByTransId(string transId, int siteId,int currencyId)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", transId);
            parameter.Add("_SiteId", siteId);
            parameter.Add("_CurrencyId", currencyId);

            return _reportDbClient.Query<GetWithdrawalBankAccountingModel>("Admin_WithdrawalBankAccounting_Get_V3",
                parameter);
        }

        public GetWithdrawalBankAccountingModel GetById(int id)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", id);

            return _reportDbClient.QueryFirst<GetWithdrawalBankAccountingModel>(
                "Admin_WithdrawalBankAccountingById_Get_V2", parameter);
        }

        public IEnumerable<WithdrawalAccountingModel> GetByTransId(string transId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TransId", transId);

            return _reportDbClient.Query<WithdrawalAccountingModel>("Admin_WithdrawalBankAccountingByTransId_Get", parameter);
        }
        
        public bool SetFlow(ActionType action, WithdrawalBankAccountingModel model)
        {
            using var connection = _mainDbClient.GetMySqlConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            switch (action)
            {
                case ActionType.Create: //Insert
                    if (Insert(model, transaction) == 0)
                    {
                        _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                        return false;
                    }

                    break;
                case ActionType.Update: //Update
                    if (Update(model, transaction) == 0)
                    {
                        _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                        return false;
                    }

                    break;
                case ActionType.Delete: //Delete
                    if (Delete(model, transaction) == 0)
                    {
                        _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                        return false;
                    }

                    break;
                case ActionType.Read:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            if (InsertLog((int) action, model, transaction) == 0)
            {
                _mainDbClient.RollbackAndDisposeConnection(connection, transaction);
                return false;
            }

            _mainDbClient.CommitAndDisposeConnection(connection, transaction);
            return true;
        }

        public int Insert(WithdrawalBankAccountingModel model,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", model.TransId);
            parameter.Add("_Seq", model.Seq);
            parameter.Add("_BankAccountNo", model.BankAccountNo);
            parameter.Add("_Amount", model.Amount);
            parameter.Add("_ChargeFee", model.ChargeFee);
            parameter.Add("_UploadPicTimes", model.UploadPicTimes);
            parameter.Add("_Memo", model.Memo);
            parameter.Add("_Creator", model.Creator);
            
            const string spName = "Admin_WithdrawalBankAccounting_Ins";
            return transaction != null
                ? _mainDbClient.Execute(spName, transaction, parameter)
                : _mainDbClient.Execute(spName, parameter);
        }

        public int InsertLog(int action, WithdrawalBankAccountingModel model,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_TransId", model.TransId);
            parameter.Add("_Seq", model.Seq);
            parameter.Add("_BankAccountNo", model.BankAccountNo);
            parameter.Add("_Amount", model.Amount);
            parameter.Add("_ChargeFee", model.ChargeFee);
            parameter.Add("_UploadPicTimes", model.UploadPicTimes);
            parameter.Add("_Memo", model.Memo);
            parameter.Add("_Mode", action);
            parameter.Add("_Creator", model.Creator);
            
            const string spName = "Admin_WithdrawalBankAccountingLog_Ins";
            return transaction != null
                ? _mainDbClient.Execute(spName, transaction, parameter)
                : _mainDbClient.Execute(spName, parameter);
        }

        private int Update(WithdrawalBankAccountingModel model,
            MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_Id", model.Id);
            parameter.Add("_TransId", model.TransId);
            parameter.Add("_Seq", model.Seq);
            parameter.Add("_BankAccountNo", model.BankAccountNo);
            parameter.Add("_Amount", model.Amount);
            parameter.Add("_ChargeFee", model.ChargeFee);
            parameter.Add("_UploadPicTimes", model.UploadPicTimes);
            parameter.Add("_Memo", model.Memo);
            parameter.Add("_Operator", model.Creator);

            const string spName = "Admin_WithdrawalBankAccounting_Upd";
            return transaction != null
                ? _mainDbClient.Execute(spName, transaction, parameter)
                : _mainDbClient.Execute(spName, parameter);
        }

        private int Delete(WithdrawalBankAccountingModel model, MySqlTransaction transaction = null)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_Id", model.Id);
            
            const string spName = "Admin_WithdrawalBankAccounting_Del";
            return transaction != null
                ? _mainDbClient.Execute(spName, transaction, parameter)
                : _mainDbClient.Execute(spName, parameter);
        }

        #endregion
    }
}