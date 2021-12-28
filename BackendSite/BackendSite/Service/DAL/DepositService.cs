using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using BackendSite.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class DepositService
    {
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;
        private readonly Common common;
        private readonly WithdrawalLimitService withdrawalLimitService;
        private int payTypePosition = 100;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly ILogger<DepositService> _logger;

        public DepositService(IServiceProvider provider)
        {
            configuration = provider.GetRequiredService<IConfiguration>();
            walletService = provider.GetRequiredService<WalletService>();
            common = provider.GetRequiredService<Common>();
            withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _logger = provider.GetRequiredService<ILogger<DepositService>>();
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, DateTime startDate, DateTime endDate, int siteId, string DepositId, string userName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelStatus,int pageSize, int amountType, string amount, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> currencyAmount)
        {
            var SelectSite = JsonConvert.SerializeObject(selectSite);
            var DepositType = JsonConvert.SerializeObject(depositType);
            var CurrencyId = JsonConvert.SerializeObject(currencyId);
            var Status = JsonConvert.SerializeObject(status);
            var VipStatus = JsonConvert.SerializeObject(levelStatus);
            var depositId = 0;
            decimal tempAmount = 0;
            if (!string.IsNullOrWhiteSpace(amount))
            {
                tempAmount = Convert.ToDecimal(amount);
            }
            var tempCurrencyTotalAmount = new List<string>();
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                var m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            
            var p = new DynamicParameters(new { SelectSite, DepositType, StartDate = startDate, EndDate = endDate, SiteId = siteId, DepositId = depositId, TransId = DepositId,
                UserName = userName, CurrencyId, Status, VipStatus= VipStatus, PageSize = pageSize, AmountType = amountType, Amount = tempAmount });

            #region GetCurrencyTotalAmount
            
            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_DepositTotalAmount_Get_V3", p).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            currencyAmount = tempCurrencyTotalAmount;
            
            #endregion

            p.Add("PageNumber", pageNumber, DbType.Int32,ParameterDirection.InputOutput);
            p.Add("StartIndex",Math.Abs(pageNumber - 1) * Math.Abs(pageSize),DbType.Int32, ParameterDirection.Input);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var depositList = _reportDbClient.Query<DepositRequest>("Admin_DepositList_Get_V7", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId,
            string TransId,
            int? Status,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount,
            out decimal totalAmount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", CustId, dbType: DbType.Int32);
            p.Add("_TransId", TransId, dbType: DbType.String);
            p.Add("_Status", Status, dbType: DbType.Int32);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_TotalAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<DepositRequest>("Admin_DepositInfoListByCust_Get_V5", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            totalAmount = p.Get<decimal?>("_TotalAmount") ?? 0;
            return depositList;
        }

        public DepositRequest GetDepositRequest(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return _mainDbClient.QueryFirst<DepositRequest>("Admin_DepositRequestTransID_Get_V2", new DynamicParameters(new { DepositId = depositId/*, SiteId = siteId*/ }));
        }

        public IEnumerable<Comments> GetDepositRequestComment(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return _mainDbClient.Query<Comments>("DepositRequestComment_Get_V2", new DynamicParameters(new { DepositId = depositId, SiteId = siteId }));
        }

        public string InsertDepositRequest(DepositDetailInfo deposit)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                deposit.CustId,
                deposit.UserName,
                deposit.CurrencyId,
                deposit.SiteId,
                deposit.DepositInfo,
                deposit.Amount,
                deposit.Comment,
                deposit.RefId,
                deposit.DepositTime,
                deposit.Operator,
                Prefix = $"D{common.GetEnv()}"
            });

            var DepositInfo = JsonConvert.DeserializeObject(deposit.DepositInfo);
            var para = new
            {
                deposit.CustId,
                deposit.UserName,
                deposit.CurrencyId,
                deposit.SiteId,
                DepositInfo,
                deposit.Amount,
                deposit.Comment,
                deposit.RefId,
                deposit.DepositTime,
                deposit.Operator,
                Prefix = $"D{common.GetEnv()}"
            };
            var propsLog = new DynamicParameters();
            propsLog.Add("Menu", "DepositSubmission");
            propsLog.Add("ActionType", 1);
            propsLog.Add("DetailInfo", JsonConvert.SerializeObject(para));
            propsLog.Add("Operator", deposit.Operator);

            conn.Execute("Admin_AdjustmentInfoLog_Set", propsLog, transaction: trans, commandType: CommandType.StoredProcedure);

            p.Add("vTransId", dbType: DbType.String, direction: ParameterDirection.Output);


            var result = conn.Execute("Admin_DepositInfo_Ins", p, transaction: trans, commandType: CommandType.StoredProcedure);
            if (result > 0)
            {
                trans.Commit();
            }

            var transId = p.Get<string>("vTransId");

            return transId;
        }

        public int SetDepositReceiptPic(int custId, int depositId, string picFileName, byte[] depositPic)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_DepositReceiptPic_Set", new { CustId = custId, DepositId = depositId, PicFileName = picFileName, DepositPic = depositPic }, commandType: CommandType.StoredProcedure);
        }

        public int SetDepositInfo(string transId, int siteId, decimal actualDeposit, string comment, string Operator, IEnumerable<int> forStatus, DepositStatusEnum status, DepositRequest depositRequest)
        {
            var type = 1;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);

            var result = 0;
            try
            {
                using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();
                var sForStatus = JsonConvert.SerializeObject(forStatus);
                var p = new DynamicParameters(new { DepositId = depositId, SiteId = siteId, ActualDeposit = actualDeposit, Comment = comment, ForStatus = sForStatus, Status = (int)status, ThirdId = (string)null, Operator = Operator });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                result = _mainDbClient.Execute("Admin_DepositInfo_Set", p, conn, trans);

                if (result > 0)
                {
                    SetDepositLog(depositId, siteId, comment, (int)status, actualDeposit, Operator);

                    if (status == DepositStatusEnum.Approved)
                    {
                        if (depositRequest.DepositType != 3)
                        {
                            var pay = (IPay)depositRequest.oDepositInfo;
                            type = payTypePosition + pay.PayId;
                        }

                        var custId = p.Get<int?>("CustId") ?? 0;
                        var userName = p.Get<string>("UserName");
                        result = walletService.SetBalance(custId, siteId, userName, actualDeposit, type, $"Deposit Approved: {actualDeposit}", transId, out _, out var is1stDepoist, conn, trans);

                        if (result > 0)
                        {
                            result = withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, actualDeposit, $"WithdrawalLimit: {actualDeposit}", transId, out _, conn, trans);
                        }

                        if (is1stDepoist == 1 && result > 0)
                        {
                            result = SetDepositInfoFirstDeposit(depositId, siteId, conn, trans);
                        }

                        if (result > 0)
                        {
                            if (depositRequest.DepositType == 3)
                            {
                                var depositInfo3 = (DepositInfo3_1)depositRequest.oDepositInfo;
                                result = walletService.SetBankOffDummy(depositRequest.SiteId, depositInfo3.SysId, depositInfo3.AccountNo, depositInfo3.CurrencyId, actualDeposit, conn, trans);
                            }
                            else if (depositRequest.oDepositInfo is IPay)
                            {
                                var pay = (IPay)depositRequest.oDepositInfo;
                                result = walletService.SetPayDummy(depositRequest.SiteId, pay.PayId, pay.CurrencyId, actualDeposit, conn, trans);
                            }
                        }
                    }

                    if (result > 0)
                    {
                        trans.Commit();
                        result = p.Get<int?>("CustId") ?? result;
                    }
                }
            }
            catch (Exception ex)
            {
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatusEnum.Error, actualDeposit, Operator);
            }

            return result;
        }

        private int SetDepositInfoFirstDeposit(int depositId, int siteId, MySqlConnection conn, MySqlTransaction trans)
        {
            return _mainDbClient.Execute("DepositInfoFirstDeposit_Set",
                new DynamicParameters(new
                {
                    DepositId = depositId,
                    SiteId = siteId
                }),
                conn, trans);
        }

        private int SetDepositLog(int depositId, int siteId, string comment, int status, decimal actualDeposit, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return _mainDbClient.Execute("DepositLog_Ins",
                new DynamicParameters(new
                {
                    DepositId = depositId, 
                    SiteId = siteId, 
                    Comment = comment, 
                    Action = status,
                    ActualDeposit = actualDeposit,
                    Operator
                }));
        }

        public DepositReceipt GetDepositReceipt(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<DepositReceipt>("DepositReceipt_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string depositType, DateTime startDate, DateTime endDate, string DepositId, string userName, string currencyId, string status,string levelStatus, int amountType, string amount)
        {
            var depositId = 0;
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                var m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            decimal tempAmount = 0;
            if (!string.IsNullOrWhiteSpace(amount))
            {
                tempAmount = Convert.ToDecimal(amount);
            }
            var p = new DynamicParameters(new { selectSite, depositType, StartDate = startDate, EndDate = endDate, DepositId = depositId, TransId = DepositId,
                UserName = userName, currencyId, status, VipStatus = levelStatus, AmountType = amountType, Amount = tempAmount });
            var depositList = _reportDbClient.Query<DepositInfoForExport>("Admin_DepositListForExport_Get_V8", p);
            return depositList;
        }
        
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType)
        {
            return _mainDbClient.Query<DepositLimitCurrencyInfo>("Admin_DepositCurrencyList_Get", new DynamicParameters(new { SiteId = siteId, DepositType = depositType }));
        }
        
        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int depositType, int currencyId)
        {
            return _mainDbClient.Query<DepositLimitInfo>("Admin_DepositLimitInfo_Get", new DynamicParameters(new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId }));
        }

        public bool AddDepositLimitInfo(DepositLimitInfo depositLimitInfo, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                depositLimitInfo.SiteId,
                depositLimitInfo.DepositType,
                depositLimitInfo.CurrencyId,
                depositLimitInfo.VipLevel,
                depositLimitInfo.DepositMin,
                depositLimitInfo.DepositMax
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "DepositLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator }, tran, commandType: CommandType.StoredProcedure);
            var result = conn.Execute("Admin_DepositLimitInfo_Ins", param, tran, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public bool EditDepositLimitInfo(DepositLimitInfo depositLimitInfo, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                depositLimitInfo.SiteId,
                depositLimitInfo.DepositType,
                depositLimitInfo.CurrencyId,
                depositLimitInfo.VipLevel,
                depositLimitInfo.DepositMin,
                depositLimitInfo.DepositMax,
                depositLimitInfo.DepositStatus
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "DepositLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator }, tran, commandType: CommandType.StoredProcedure);
            var result = conn.Execute("Admin_DepositLimitInfo_Set", param, tran, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public DepositLimitInfo GetDepositLimitInfo(int siteId, int depositType, int currencyId, int vipLevel)
        {
            return _mainDbClient.QueryFirst<DepositLimitInfo>("Admin_DepositLimitInfoDetail_Get", new DynamicParameters(new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId, VipLevel = vipLevel }));
        }

        public int DeleteDepositLimitInfo(DepositLimitInfo depositLimitInfo, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                depositLimitInfo.SiteId,
                depositLimitInfo.CurrencyId,
                depositLimitInfo.VipLevel,
                depositLimitInfo.DepositType
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "DepositLimitManagement", ActionType = 2, DetailInfo = JsonConvert.SerializeObject(param), Operator }, tran, commandType: CommandType.StoredProcedure);
            var result = conn.Execute("Admin_DepositLimitInfo_Del", new { depositLimitInfo.SiteId,  depositLimitInfo.CurrencyId,  depositLimitInfo.VipLevel, depositLimitInfo.DepositType }, commandType: CommandType.StoredProcedure);
            tran.Commit();

            return result;
        }

        public DepositTimeData DepositTimeReportGet(DepositTimeReportPara para)
        {
            var selectSite = JsonConvert.SerializeObject(para.SiteIds);
            var selectStatus = new List<int>();
            if(para.Status == 0)
            {
                selectStatus.Add(3);
                selectStatus.Add(4);
                selectStatus.Add(5);
            }
            else
            {
                selectStatus.Add(para.Status);
            }
            var SelectStatus = JsonConvert.SerializeObject(selectStatus);

            var searchData = new DepositTimeData();
            var props = new DynamicParameters();
            int.TryParse(para.DepositId, out var tempDepositId);
            
            props.Add("SelectSite", selectSite);
            props.Add("DepositId", tempDepositId);
            props.Add("StartDate", para.StartDate.DateTime);
            props.Add("EndDate", para.EndDate.DateTime);
            props.Add("SelectStatus", SelectStatus);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("StartIndex", (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize), DbType.Int32,ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(para.PageNumber), DbType.Int32,ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", Math.Abs(para.PageSize));
            var result = _reportDbClient.Query<DepositTimeList>("Admin_DepositStatusTime_Get_V3", props).ToList();

            searchData.DepositTimeList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return searchData;
        }
        public List<DepositTimeList> GetExportData(IEnumerable<int> SelectSite, string depositId, DateTime startDate, DateTime endDate, int status, int currencyId)
        {
            var selectSite = JsonConvert.SerializeObject(SelectSite);
            var selectstatus = new List<int>();
            if (status == 0)
            {
                selectstatus.Add(3);
                selectstatus.Add(4);
                selectstatus.Add(5);
            }
            else
            {
                selectstatus.Add(status);
            }
            var selectStatus = JsonConvert.SerializeObject(selectstatus);
            int.TryParse(depositId, out var tempDepositId);

            var props = new DynamicParameters();
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("SelectSite", selectSite);
            props.Add("CurrencyId", currencyId);
            props.Add("DepositId", tempDepositId);
            props.Add("SelectStatus", selectStatus);
            return _reportDbClient.Query<DepositTimeList>("Admin_DepositStatusTimeExport_Get_V3", props).ToList();

        }
        public bool SetDepositRequestAmount(string transId, decimal amount, string Operator)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);

            var props = new DynamicParameters();
            props.Add("_DepositId", depositId);
            props.Add("_Amount", amount);
            props.Add("_Operator", Operator);
            
            return _mainDbClient.Execute("Admin_DepositRequestAmountV2_Set", props) > 0;
        }
    }
}
