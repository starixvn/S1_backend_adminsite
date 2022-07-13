using System;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class CustInfoService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public CustInfoService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public Customer GetCustInfo(int custId)
        {
            return _reportDbClient.QueryFirst<Customer>("Customer_Get", new DynamicParameters(new { CustId = custId }));
        }

        public IEnumerable<CustInfoDetail> GetCustInfoList(IEnumerable<int> selectSite,
            int? birDate,
            int? birMonth,
            int? birYear,
            string userName,
            string accountName,
            string loginIp,
            string wTransId,
            string dTransId,
            string tTransId,
            string tpUserName,
            string email,
            string phoneNo,
            DateTime? startDate,
            DateTime? endDate,
            DateTime? tagStartDate,
            DateTime? tagEndDate,
            DateTime? statusStartDate,
            DateTime? statusEndDate,
            int pageSize,
            string affCode,
            IEnumerable<int> selectCurrency,
            string bankAccountNumber,
            IEnumerable<int> selectStatus,
            IEnumerable<int> selectTag,
            ref int pageNumber,
            out int totalPages,
            out int rowCount,
            string siteList,
            string currencyList)
        {
            string jsonSelectSite = null;
            string jsonSelectCurrency = null;
            var jsonSelectStatus = string.Empty;
            var jsonSelectTagList = string.Empty;

            if (selectSite != null && selectSite.Any())
            {
                jsonSelectSite = JsonConvert.SerializeObject(selectSite);
            }
            if (selectCurrency != null && selectCurrency.Any())
            {
                jsonSelectCurrency = JsonConvert.SerializeObject(selectCurrency);
            }
            if (selectStatus != null)
            {
                if (selectStatus.ToList().Count == 0)
                {
                    var tempList = new List<int>() { 1, 2, 3 };
                    jsonSelectStatus = JsonConvert.SerializeObject(tempList);
                }
                else
                {
                    jsonSelectStatus = JsonConvert.SerializeObject(selectStatus);
                }
            }
            if (selectTag != null && selectTag.Any())
            {
                jsonSelectTagList = JsonConvert.SerializeObject(selectTag);
            }
            else
            {
                jsonSelectTagList = null;
            }
            totalPages = 0;


            var p = new DynamicParameters(new
            {
                SelectSite = jsonSelectSite,
                BirDate = birDate,
                BirMonth = birMonth,
                BirYear = birYear,
                UserName = userName,
                AccountName = accountName,
                LoginIp = loginIp,
                WTransId = wTransId,
                DTransId = dTransId,
                TTransId = tTransId,
                TpUserName = tpUserName,
                Email = email,
                PhoneNo = phoneNo,
                StartDate = startDate,
                EndDate = endDate,
                TagStartDate = tagStartDate,
                TagEndDate = tagEndDate,
                StatusStartDate = statusStartDate,
                StatusEndDate = statusEndDate,
                PageSize = pageSize,
                SelectCurrency = jsonSelectCurrency,
                SiteList = siteList,
                AffCode = affCode,
                CurrncyList = currencyList,
                BankAccountNumber = bankAccountNumber,
                SelectStatus = jsonSelectStatus,
                SelectTagList = jsonSelectTagList
            });
            p.Add("PageNumber", pageNumber, DbType.Int32, ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<CustInfoDetail>("Admin_CustInfoSearch_Get_V12", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public IEnumerable<T> GetCustInfoExportList<T>(IEnumerable<int> selectSite,
            int? birDate,
            int? birMonth,
            int? birYear,
            string userName,
            string accountName,
            string loginIp,
            string wTransId,
            string dTransId,
            string tTransId,
            string tpUserName,
            string email,
            string phoneNo,
            DateTime? startDate,
            DateTime? endDate,
            DateTime? tagStartDate,
            DateTime? tagEndDate,
            DateTime? statusStartDate,
            DateTime? statusEndDate,
            string siteList,
            string affCode,
            IEnumerable<int> selectCurrency,
            string bankAccountNumber,
            string currencyList,
            IEnumerable<int> selectStatus,
            IEnumerable<int> selectTag)
        {
            string SelectSite = null;
            string SelectCurrency = null;
            string SelectStatus = null;
            var jsonSelectTagList = string.Empty;

            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            if (selectCurrency != null && selectCurrency.Any())
            {
                SelectCurrency = JsonConvert.SerializeObject(selectCurrency);
            }
            if (selectStatus != null)
            {
                if (selectStatus.ToList().Count == 0)
                {
                    var templist = new List<int>() { 1, 2, 3 };
                    SelectStatus = JsonConvert.SerializeObject(templist);
                }
                else
                {
                    SelectStatus = JsonConvert.SerializeObject(selectStatus);
                }
            }
            if (selectTag != null && selectTag.Any())
            {
                jsonSelectTagList = JsonConvert.SerializeObject(selectTag);
            }
            else
            {
                jsonSelectTagList = null;
            }

            var p = new DynamicParameters(new
            {
                SelectSite,
                BirDate = birDate,
                BirMonth = birMonth,
                BirYear = birYear,
                UserName = userName,
                AccountName = accountName,
                LoginIp = loginIp,
                TpUserName = tpUserName,
                WTransId = wTransId,
                DTransId = dTransId,
                TTransId = tTransId,
                Email = email,
                PhoneNo = phoneNo,
                StartDate = startDate,
                EndDate = endDate,
                TagStartDate = tagStartDate,
                TagEndDate = tagEndDate,
                StatusStartDate = statusStartDate,
                StatusEndDate = statusEndDate,
                SiteList = siteList,
                AffCode = affCode,
                BankAccountNumber = bankAccountNumber,
                SelectCurrency = SelectCurrency,
                CurrncyList = currencyList,
                SelectStatus = SelectStatus,
                SelectTagList = jsonSelectTagList
            });

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var custInfoDetailList = conn.Query<T>("Admin_CustInfoSearchForExport_Get_V9", p, commandType: CommandType.StoredProcedure);
            return custInfoDetailList;
        }

        public int GetVipLevel(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.ExecuteScalar<int>("VipLevel_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName, string siteList)
        {
            var result = _reportDbClient.QueryFirst<CustInfoDetail>("Admin_CustInfoDetailSearch_Get_V10", new DynamicParameters(new { CustId = custId, UserName = userName, SiteList = siteList }));
            return result;
        }

        public CustInfoDetail GetCustInfos(int? custId, string userName, int? siteId)
        {
            return _mainDbClient.QueryFirst<CustInfoDetail>("Admin_CustInfo_Get", new DynamicParameters(new { CustId = custId, UserName = userName, SiteId = siteId }));
        }

        public bool SendMessageBatch(BatchPersonalMsg req)
        {
            return _mainDbClient.Execute("Admin_SendMsgBatchByStatus_Ins_V3",
                new DynamicParameters(new
                {
                    _BatchId = req.BatchId,
                    _SiteId = req.SiteId,
                    _LangId = req.LangId,
                    _CurrencyId = req.CurrencyId,
                    _Status = req.AccountStatus,
                    _IsPopup = req.IsPopup,
                    _Operator = req.Operator
                }), 600) > 0;
        }


        public LastDepositAndWithdrawalTime GetCustDepositAndWithdrawalTime(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.QueryFirstOrDefault<LastDepositAndWithdrawalTime>("Admin_CustInfoOther", new { CustId = custId }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public int SetCustStatusSwitch(int custId, int Op)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustStatusSwitch_Set", new { CustId = custId, Op }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, Dictionary<string, string> address, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustInfo_Set_V2", new { CustId = custId, Level, FullName, Birthday_Y, Birthday_M, Birthday_D, PhoneNumber, EMail, Address = JsonConvert.SerializeObject(address), Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public CustPhotoId GetCustPhotoId(int CustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<CustPhotoId>("Admin_CustPhotoId_Get", new { CustId = CustId }, commandType: CommandType.StoredProcedure);
        }

        public int SetCustPhotoId(int custId, int siteId, string picFileName, byte[] photoIdPic)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustPhotoId_Ins", new { CustId = custId, SiteId = siteId, PicFileName = picFileName, PhotoIdPic = photoIdPic }, commandType: CommandType.StoredProcedure);
        }

        public bool DelCustPhotoId(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustPhotoId_Del", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool AddCustNote(int custId, string Note, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustNotes_Ins", new { CustId = custId, Note = Note, Operator = Operator }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<CustNote> GetCustNotes(int custId,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameter = new DynamicParameters();

            parameter.Add("_CustId", custId);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = _reportDbClient.Query<CustNote>("Admin_CustNotes_Get_V2", parameter);


            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }
        public bool SetCustNotes(int noteId, string note, int type, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustNotes_Set_V2", new { NoteId = noteId, Note = note, Type = type, Operator = Operator }, commandType: CommandType.StoredProcedure) > 0;
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int custId, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int startIndex, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var depositList = conn.Query<LoginLog>("Admin_LoginLogByCust_Get_V3", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return depositList;
        }
        public bool SetResetPwd(int custId, string md5NewPassword)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetPwd_Set", new { CustId = custId, Md5NewPassword = md5NewPassword }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetProfile(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetProfile_Set_V2", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetCustBankInfo(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetCustBankInfo_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetCustUSDTInfo(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetCustUSDTInfo_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetCustStatus(int custId, int status, int taskId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustStatus_Set_V2", new { CustId = custId, Status = status, TaskId = taskId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);
            var result = GetWithdrawalAvailableBalance(custId, conn, trans);
            trans.Rollback();
            return result;
        }
        public decimal GetWithdrawalAvailableBalance(int custId, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.ExecuteScalar<decimal>("Common_WithdrawalAvailableBalance_Get", new { CustId = custId }, transaction: trans, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<CustBankInfoLog> GetCustBankInfoLogList(int custId,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameter = new DynamicParameters();

            parameter.Add("_CustId", custId);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = conn.Query<CustBankInfoLog>("Admin_BankInfoLogList_Get_V3", parameter, commandType: CommandType.StoredProcedure);

            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }
        public IEnumerable<CustCryptoWalletLog> GetCustCryptoWalletLogList(int custId,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var parameter = new DynamicParameters();

            parameter.Add("_CustId", custId);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = conn.Query<CustCryptoWalletLog>("Admin_CryptoWalletLogList_Get_V2", parameter, commandType: CommandType.StoredProcedure);

            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }
        public IEnumerable<CustBonus> GetCustBonus(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustBonus>("Admin_CustBonus_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public class MutiSelect
        {
            public string label { get; set; }
            public int value { get; set; }
        }

        public TeleCSDData GetTeleCSD(string siteIds, string currency, DateTimeOffset startDate, DateTimeOffset endDate, int deposit, int dormantUser, int callOut, int pageNumber, int pageSize)
        {
            var searchData = new TeleCSDData();
            var props = new DynamicParameters();
            props.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32);
            props.Add("PageSize", Math.Abs(pageSize));
            props.Add("SelectSite", siteIds);
            props.Add("CurrencyList", currency);
            props.Add("StartDate", startDate.DateTime);
            props.Add("EndDate", endDate.DateTime);
            props.Add("ActStatus", deposit);
            props.Add("DayInactive", dormantUser);
            props.Add("CallOutStatus", callOut);
            var result = _reportDbClient.Query<CustTele>("Admin_CustTeleSearch_Get_V4", props).ToList();

            result.ForEach(x =>
            {
                if (x.UpdateAt != null)
                {
                    var updateAtArray = x.UpdateAt.Split(',');
                    var operatorArray = x.Operator.Split(',');
                    x.UpdateRemark = new List<string>();
                    for (var i = 0; i < updateAtArray.Length; i++)
                    {
                        x.UpdateRemark.Add($"{updateAtArray[i]}-{operatorArray[i]}");
                    }
                    x.CallOutStatus = updateAtArray.Length;
                    if (updateAtArray.Length == 3)
                    {
                        x.CanUpdate = false;
                    }
                }
            });

            searchData.CustTeleList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return searchData;
        }

        public int UpdateCustomerTele(int siteId, int custId, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", siteId);
            props.Add("CustId", custId);
            props.Add("Operator", @operator);
            return _mainDbClient.Execute("Admin_CustTele_Ins", props);
        }

        public List<CustTeleExport> GetTeleCSDExportData(string siteIds, string currency, DateTimeOffset startDate, DateTimeOffset endDate, int deposit, int dormantUser, int callOut)
        {
            var result = new List<CustTeleExport>();
            var props = new DynamicParameters();
            props.Add("SelectSite", siteIds);
            props.Add("CurrencyList", currency);
            props.Add("StartDate", startDate.DateTime);
            props.Add("EndDate", endDate.DateTime);
            props.Add("ActStatus", deposit);
            props.Add("DayInactive", dormantUser);
            props.Add("CallOutStatus", callOut);
            var tempResult = _reportDbClient.Query<CustTeleExportTemp>("Admin_CustTeleExport_Get_V4", props).ToList();

            tempResult.ForEach(x =>
            {
                var firstCall = string.Empty;
                var secondCall = string.Empty;
                var thirdCall = string.Empty;
                if (x.UpdateAt != null && x.Operator != null)
                {
                    var updateAtArray = x.UpdateAt.Split(',');
                    var operatorArray = x.Operator.Split(',');
                    for (var i = 0; i < updateAtArray.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                firstCall = $"{updateAtArray[i]}-{operatorArray[i]}";
                                break;
                            case 1:
                                secondCall = $"{updateAtArray[i]}-{operatorArray[i]}";
                                break;
                            case 2:
                                thirdCall = $"{updateAtArray[i]}-{operatorArray[i]}";
                                break;
                        }
                    }
                }

                result.Add(new CustTeleExport()
                {
                    UserName = x.UserName,
                    FullName = x.FullName,
                    DepositStatus = x.DepositStatus,
                    RegDate = x.RegDate,
                    PhoneNumber = x.PhoneNumber,
                    FirstTimeCall = firstCall,
                    SecondTimeCall = secondCall,
                    ThirdTimeCall = thirdCall
                });
            });

            return result;
        }

        /// <summary>
        /// 刪除線上使用者資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteOnUserData(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId
                });

            var SpName = "Admin_OnUser_Del";
            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// 刪除使用者Line資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteLineProfile(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId
                });

            var SpName = "Admin_CustLineProfile_Del";
            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        public CustLineProfileModel GetCustLineProfile(int custId, string lineUserId)
        {
            return _reportDbClient.QueryFirst<CustLineProfileModel>("Member_CustLineProfile_Get", new DynamicParameters(new
            {
                _CustId = custId,
                _userId = lineUserId
            }));
        }


        #region Tag
        public bool EditTagManagement(ParamsWithLog<EditTagManagement> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var props = new DynamicParameters();
            props.Add("Id", input.After.Id);
            props.Add("TagName", input.After.TagName);
            props.Add("TagColour", input.After.TagColour);
            props.Add("Description", input.After.Description);
            props.Add("Operator", input.Operator);

            var success = conn
                .Execute("Admin_Tag_Set", props, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var isCreate = input.After.Id == 0;
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.TagManagement);
                logParam.Add("_ActionType", (int) (isCreate ? ActionType.Create : ActionType.Update));
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog(), DbType.String);
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }

            }

            trans.Rollback();

            return false;
        }

        public GetTagDataResult GetTagData(int status, int pageNumber, int pageSize)
        {
            var searchResult = new GetTagDataResult();
            var props = new DynamicParameters();
            props.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", Math.Abs(pageSize));
            props.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            props.Add("Status", status);

            var result = _reportDbClient.Query<TagData>("Admin_Tag_Get_V3", props);
            searchResult.TagDatas = result.ToList();
            searchResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return searchResult;
        }

        public bool SetTagStatus(ParamsWithLog<SetTagStatus> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var props = new DynamicParameters();
            props.Add("Id", input.After.Id);
            props.Add("Operator", input.Operator);

            var success = conn
                .Execute("Admin_TagStatus_Set", props, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.TagManagement);
                logParam.Add("_ActionType", (int) ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }

        public IEnumerable<TagData> GetAllTag()
        {

            return _reportDbClient.Query<TagData>("Admin_Tag_GetAll");
        }

        public int SetCustTag(int custid, IEnumerable<int> SelectTag)
        {
            var temp = new List<int>();
            var selectTag = JsonConvert.SerializeObject(temp);
            if (SelectTag != null && SelectTag.Any())
            {
                selectTag = JsonConvert.SerializeObject(SelectTag);
            }
            var props = new DynamicParameters();
            props.Add("CustId", custid);
            props.Add("TagList", selectTag);

            return _mainDbClient.Execute("Admin_CustInofoTagList_Set", props);
        }
        #endregion

        public bool SetEnableDisablePromotion(int custId, int enablePromotion)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustInofoEnablePromotion_Set", new { CustId = custId, EnablePromotion = enablePromotion }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<VipLevelModel> GetAllSiteVipLevelList()
        {
            return _reportDbClient.Query<VipLevelModel>("Common_VipLevel_Get_V2");
        }

        public RegLiveStatusResult GetRegLiveStatusData(IEnumerable<int> selectSite, int currency, int depositStatus, int status, string username, int pageNumber, int pageSize, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            var searchData = new RegLiveStatusResult();
            var props = new DynamicParameters();
            var creditSite = new List<string> { "Nova88Ref01", "Nova88Ref02", "Nova88Ref03", "Nova88Ref04", "Nova88Ref05", "Nova88Ref06", "Nova88Ref07", "Nova88Ref08", "Goal123Ref01", "BwingRef01" };
            props.Add("SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(username) ? null : username);
            props.Add("DepositStatus", depositStatus);
            props.Add("Status", status);
            props.Add("Sort", sort);
            props.Add("PageSize", Math.Abs(pageSize));
            props.Add("StartDate", startDate.DateTime);
            props.Add("EndDate", endDate.DateTime);
            props.Add("AffCode", string.IsNullOrWhiteSpace(affiliatesCode) ? null : affiliatesCode);
            props.Add("CreditSite", JsonConvert.SerializeObject(creditSite));
            props.Add("AgentCodeType", agentCodeType);
            props.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<RegLiveStatus>("Admin_CustRegLiveStatus_Get_V5", props).ToList();
            searchData.RegLiveStatusDatas = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return searchData;
        }

        public bool SetRegLiveStatus(ParamsWithLog<SetRegLiveStatus> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            
            var props = new DynamicParameters();
            props.Add("CustId", input.After.CustId);
            props.Add("Status", (int)input.After.Status);
            props.Add("SiteId", input.SiteId);
            props.Add("Operator", input.Operator);

            var success = conn
                .Execute("Admin_RegLiveStatus_Set", props, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics);
                logParam.Add("_ActionType", (int) ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }

        public IEnumerable<RegLiveStatusNotes> GetRegLiveStatusNotes(int custId)
        {
            return _reportDbClient.Query<RegLiveStatusNotes>("Admin_CustRegLiveStatusNotes_Get_V2", new DynamicParameters(new { CustId = custId }));
        }

        public bool AddRegLiveStatusNote(ParamsWithLog<AddRegLiveStatusNote> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            
            var props = new DynamicParameters();
            props.Add("CustId", input.After.CustId);
            props.Add("Note",  string.IsNullOrWhiteSpace(input.After.Note) ? "" : input.After.Note);
            props.Add("Operator", input.Operator);

            var success = conn
                .Execute("Admin_CustRegLiveStatusNotes_Ins", props, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics);
                logParam.Add("_ActionType", (int) ActionType.Create);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", null, DbType.String);
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }

        public bool DeleteRegLiveStatusNote(ParamsWithLog<DeleteRegLiveStatusNote> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            
            var props = new DynamicParameters();
            props.Add("CustId", input.Before.CustId);
            props.Add("Id",  input.Before.Id);

            var success = conn
                .Execute("Admin_CustRegLiveStatusNotes_Del", props, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics);
                logParam.Add("_ActionType", (int) ActionType.Delete);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", null, DbType.String);
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }

        public bool EditRegLiveStatusNote(ParamsWithLog<EditRegLiveStatusNote> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            
            var props = new DynamicParameters();
            props.Add("CustId", input.After.CustId);
            props.Add("Id",  input.After.Id);
            props.Add("Note",  input.After.Note);
            props.Add("Operator", input.Operator);

            var success = conn
                .Execute("Admin_CustRegLiveStatusNotes_Set", props, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.LiveRegistrationStatistics);
                logParam.Add("_ActionType", (int) ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }
        public IEnumerable<RegLiveStatus> GetExportRegLiveStatus(string siteIds, int currency, int depositStatus, int status, string user, int sort, DateTimeOffset startDate, DateTimeOffset endDate, string affiliatesCode, int agentCodeType)
        {
            var creditSite = new List<string>() { "Nova88Ref01", "Nova88Ref02", "Nova88Ref03", "Nova88Ref04", "Nova88Ref05", "Nova88Ref06", "Nova88Ref07", "Nova88Ref08", "Goal123Ref01", "BwingRef01" };
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("SelectSite", siteIds);
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(user) ? null : user);
            props.Add("DepositStatus", depositStatus);
            props.Add("Status", status);
            props.Add("Sort", sort);
            props.Add("StartDate", startDate.DateTime);
            props.Add("EndDate", endDate.DateTime);
            props.Add("AffCode", string.IsNullOrWhiteSpace(affiliatesCode) ? null : affiliatesCode);
            props.Add("CreditSite", JsonConvert.SerializeObject(creditSite));
            props.Add("AgentCodeType", agentCodeType);

            var result = _reportDbClient.Query<RegLiveStatus>("Admin_CustRegLiveStatusExport_Get_V4", props);

            return result;
        }

        public async Task<CustomerForRisk> GetCustId(int? custId,string userName, int? siteId)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("UserName", userName);
            props.Add("SiteId", siteId);
            return await _reportDbClient.QueryFirstAsync<CustomerForRisk>("Admin_CustInfo_Get", props);
        }

        public CustomerSwitchStatus GetCustSwitchStatus(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<CustomerSwitchStatus>("Admin_CustSwitchStatus_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public CustProfileReq GetCustProfile(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<CustProfileReq>("Admin_CustProfile_Get_V2", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<CustomerWhoWaitToSend> GetCustInfoByCust(int siteId, int? custId, string userName)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_UserName", userName);
            p.Add("_SiteId", siteId);
            return await _mainDbClient.QueryFirstAsync<CustomerWhoWaitToSend>("Admin_Msg_CustInfoByCust_Get", p);
        }

        public async Task<IEnumerable<CustomerWhoWaitToSend>> GetCustInfoBySite(int siteId, int currencyId, int status)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_CurrencyId", currencyId);
            p.Add("_Status", status);
            return await _mainDbClient.QueryAsync<CustomerWhoWaitToSend>("Admin_Msg_CustInfoBySite_Get", p);
        }

        public WithdrawalLimitLogData GetWithdrawalLimitLog(int custId, DateTimeOffset StartDate, DateTimeOffset EndDate, int PageNumber, int PageSize)
        {
            var WithdrawalLimitLogData = new WithdrawalLimitLogData();
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId, DbType.Int32);
            parameter.Add("StartDate", DateTimeOffset.Parse(StartDate.ToString()).DateTime, DbType.DateTime);
            parameter.Add("EndDate", DateTimeOffset.Parse(EndDate.ToString()).DateTime, DbType.DateTime);
            parameter.Add("PageNumber", Math.Abs(PageNumber), dbType: DbType.Int32);
            parameter.Add("StartIndex", (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize), dbType: DbType.Int32);
            parameter.Add("PageSize", Math.Abs(PageSize), DbType.Int32);
            parameter.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = _reportDbClient.Query<WithdrawalLimitLog>("Admin_WithdrawalLimitLog_Get", parameter).ToList();

            WithdrawalLimitLogData.WithdrawalLimitLogList = data;
            WithdrawalLimitLogData.TotalPages = parameter.Get<int?>("TotalPages") ?? 0;
            WithdrawalLimitLogData.RowCount = parameter.Get<int?>("RowCount") ?? 0;

            return WithdrawalLimitLogData;
        }
        
        public Task<IEnumerable<CustomerInfo>> GetCustInfosAsync(int siteId, string userNames)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_UserNames", userNames);
            
            return _reportDbClient.QueryAsync<CustomerInfo>("Admin_CustInfo_GetList", props);
        }
    }
}
