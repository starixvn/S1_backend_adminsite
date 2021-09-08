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

        public DepositService(IConfiguration configuration, WalletService walletService, Common common, WithdrawalLimitService withdrawalLimitService, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            this.walletService = walletService;
            this.common = common;
            this.withdrawalLimitService = withdrawalLimitService;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, long startDate, long endDate, int siteId, string DepositId, string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,int pageSize, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            var SelectSite = JsonConvert.SerializeObject(selectSite);
            var DepositType = JsonConvert.SerializeObject(depositType);
            var CurrencyId = JsonConvert.SerializeObject(currencyId);
            var Status = JsonConvert.SerializeObject(status);
            var VipStatus = JsonConvert.SerializeObject(levelstatus);
            var depositId = 0;
            var tempCurrencyTotalAmount = new List<string>();
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                var m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var p = new DynamicParameters(new { SelectSite, DepositType, StartDate = startDate, EndDate = endDate, SiteId = siteId, DepositId = depositId, TransId = DepositId, UserName, CurrencyId, Status, VipStatus= VipStatus, PageSize = pageSize });

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_DepositTotalAmount_Get", p, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);


            var depositList = conn.Query<DepositRequest>("Admin_DepositListV5_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId, string TransId, int? Status, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount, out decimal totalAmount)
        {
            var p = new DynamicParameters(new { CustId, TransId, Status, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("TotalAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<DepositRequest>("Admin_DepositInfoListByCustV4_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            totalAmount = p.Get<decimal?>("TotalAmount") ?? 0;
            return depositList;
        }

        public DepositRequest GetDepositRequest(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<DepositRequest>("Admin_DepositRequestTransID_Get", new { DepositId = depositId/*, SiteId = siteId*/ }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Comments> GetDepositRequestComment(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Comments>("DepositRequestComment_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
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

            var para = new
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

        public int SetDepositInfo(string transId, int siteId, decimal actualDeposit, string comment, string Operator, IEnumerable<int> forStatus, DepositStatus status, DepositRequest depositRequest)
        {
            var depositId = 0;
            var type = 1;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);

            var result = 0;
            try
            {
                using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
                conn.Open();
                using var trans = conn.BeginTransaction();
                var sForStatus = Newtonsoft.Json.JsonConvert.SerializeObject(forStatus);
                var p = new DynamicParameters(new { DepositId = depositId, SiteId = siteId, ActualDeposit = actualDeposit, Comment = comment, ForStatus = sForStatus, Status = (int)status, ThirdId = (string)null, Operator = Operator });
                p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
                result = conn.Execute("Admin_DepositInfo_Set", p, trans, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    SetDepositLog(depositId, siteId, comment, (int)status, actualDeposit, Operator);

                    if (status == DepositStatus.Approved)
                    {
                        if (depositRequest.DepositType != 3)
                        {
                            var pay = (IPay)depositRequest.oDepositInfo;
                            type = payTypePosition + pay.PayId;
                        }

                        byte is1stDepoist = 0;
                        decimal afterAmount = 0;

                        var custId = p.Get<int?>("CustId") ?? 0;
                        var UserName = p.Get<string>("UserName");
                        result = walletService.SetBalance(custId, siteId, UserName, actualDeposit, type, $"Deposit Approved: {actualDeposit}", transId, out afterAmount, out is1stDepoist, conn, trans);

                        if (result > 0)
                        {
                            decimal withdrawalLimit = 0;
                            result = withdrawalLimitService.SetWithdrawalLimit(custId, UserName, siteId, actualDeposit, $"WithdrawalLimit: {actualDeposit}", transId, out withdrawalLimit, conn, trans);
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
                SetDepositLog(depositId, siteId, ex.Message, (int)DepositStatus.Error, actualDeposit, Operator);
            }

            return result;
        }

        private int SetDepositInfoFirstDeposit(int depositId, int siteId, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("DepositInfoFirstDeposit_Set", new { DepositId = depositId, SiteId = siteId }, trans, commandType: CommandType.StoredProcedure);
        }

        private int SetDepositLog(int depositId, int siteId, string comment, int status, decimal actualDeposit, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("DepositLog_Ins", new { DepositId = depositId, SiteId = siteId, Comment = comment, Action = status, ActualDeposit = actualDeposit, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }

        public DepositReceipt GetDepositReceipt(int depositId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<DepositReceipt>("DepositReceipt_Get", new { DepositId = depositId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string depositType, long startDate, long endDate, string DepositId, string UserName, string currencyId, string status,string LevelStatus)
        {
            //var SelectSite = JsonConvert.SerializeObject(selectSite);

            var SelectSite = $"[{selectSite}]";
            var DepositType = $"[{depositType}]";
            var CurrencyId = $"[{currencyId}]";
            var Status = $"[{status}]";
            var VipStatus = $"[{LevelStatus}]";

            var depositId = 0;
            if (!string.IsNullOrWhiteSpace(DepositId))
            {
                var m = Regex.Match(DepositId, @"\d+");
                int.TryParse(m.Value, out depositId);
            }
            var p = new DynamicParameters(new { SelectSite, DepositType, StartDate = startDate, EndDate = endDate, DepositId = depositId, TransId = DepositId, UserName, CurrencyId, Status, VipStatus = VipStatus });
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<DepositInfoForExport>("Admin_DepositListForExportV6_Get", p, commandType: CommandType.StoredProcedure);
            return depositList;
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositLimitCurrencyInfo>("Admin_DepositCurrencyList_Get", new { SiteId = siteId, DepositType = depositType }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int depositType, int currencyId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DepositLimitInfo>("Admin_DepositLimitInfo_Get", new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool AddDepositLimitInfo(DepositLimitInfo depositLimitInfo, string Operator)
        {
            var result = false;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
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
                    result = conn.Execute("Admin_DepositLimitInfo_Ins", param, tran, commandType: CommandType.StoredProcedure) > 0;
                    tran.Commit();
                }
                    
            }
            return result;
        }

        public bool EditDepositLimitInfo(DepositLimitInfo depositLimitInfo,string Operator)
        {
            var result = false;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
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

                    result = conn.Execute("Admin_DepositLimitInfo_Set", param, tran, commandType: CommandType.StoredProcedure) > 0;
                    tran.Commit();
                }
                   
            }
            return result;
        }

        public DepositLimitInfo GetDepositLimitInfo(int siteId, int depositType, int currencyId, int viplevel)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<DepositLimitInfo>("Admin_DepositLimitInfoDetail_Get", new { SiteId = siteId, DepositType = depositType, CurrencyId = currencyId, VipLevel = viplevel }, commandType: CommandType.StoredProcedure);
            }
        }

        public int DeleteDepositLimitInfo(DepositLimitInfo depositLimitInfo, string Operator)
        {
            var result = 0;
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    
                    var param = new
                    {
                        depositLimitInfo.SiteId,
                        depositLimitInfo.CurrencyId,
                        depositLimitInfo.VipLevel,
                        depositLimitInfo.DepositType
                    };
                    conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "DepositLimitManagement", ActionType = 2, DetailInfo = JsonConvert.SerializeObject(param), Operator }, tran, commandType: CommandType.StoredProcedure);
                    result = conn.Execute("Admin_DepositLimitInfo_Del", new { SiteId = depositLimitInfo.SiteId, CurrencyId = depositLimitInfo.CurrencyId, VipLevel = depositLimitInfo.VipLevel, DepositType = depositLimitInfo.DepositType }, commandType: CommandType.StoredProcedure);
                    tran.Commit();
                }            
            }
            return result;
        }

        public DepositTimeData DepositTimeReportGet(DepositTimeReportPara para)
        {
            var SelectSite = JsonConvert.SerializeObject(para.SelectSite);
            var selectstatus = new List<int>();
            if(para.Status == 0)
            {
                selectstatus.Add(3);
                selectstatus.Add(4);
                selectstatus.Add(5);
            }
            else
            {
                selectstatus.Add(para.Status);
            }
            var SelectStatus = JsonConvert.SerializeObject(selectstatus);

            var SearchData = new DepositTimeData();
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            int tempDepositId;
            Int32.TryParse(para.DepositID, out tempDepositId);
            
            props.Add("SelectSite", SelectSite);
            props.Add("DepositId", tempDepositId);
            props.Add("StartDate", para.StartDate);
            props.Add("EndDate", para.EndDate);
            props.Add("SelectStatus", SelectStatus);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("PageNumber", para.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", para.PageSize);
            var result = conn.Query<DepositTimeList>("Admin_DepositStatusTime_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.DepositTimeList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
        }
        public List<DepositTimeList> GetExportData(IEnumerable<int> SelectSite, string DepositID, long StartDate, long EndDate, int Status, int CurrencyId)
        {
            var selectSite = JsonConvert.SerializeObject(SelectSite);
            var selectstatus = new List<int>();
            if (Status == 0)
            {
                selectstatus.Add(3);
                selectstatus.Add(4);
                selectstatus.Add(5);
            }
            else
            {
                selectstatus.Add(Status);
            }
            var SelectStatus = JsonConvert.SerializeObject(selectstatus);
            int tempDepositId;
            Int32.TryParse(DepositID, out tempDepositId);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("SelectSite", selectSite);
            props.Add("CurrencyId", CurrencyId);
            props.Add("DepositId", tempDepositId);
            props.Add("SelectStatus", SelectStatus);
            return conn.Query<DepositTimeList>("Admin_DepositStatusTimeExport_Get", props, commandType: CommandType.StoredProcedure).ToList();

        }
        public bool SetDepositRequestAmount(string transId, decimal amount, string Operator)
        {
            var depositId = 0;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);

            var props = new DynamicParameters();
            props.Add("_DepositId", depositId);
            props.Add("_Amount", amount);
            props.Add("_Operator", Operator);

            //return _mainDbClient.Execute("Admin_DepositRequestAmount_Set", props);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_DepositRequestAmountV2_Set", props, commandType: CommandType.StoredProcedure) > 0;
        }
    }
}
