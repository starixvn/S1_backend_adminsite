using System;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<Customer>("Customer_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<CustInfoDetail> GetCustInfoList(IEnumerable<int> selectSite,
            int? birDate,
            int? birMonth,
            int? birYear,
            string userName,
            string accountName,
            string loginIp,
            string transId,
            string tpUserName,
            string email,
            string phoneNo,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            int pageSize,
            int? affId,
            IEnumerable<int> selectCurrency,
            string bankAccountNumber,
            IEnumerable<int> selectStatus,
            ref int pageNumber,
            out int totalPages,
            out int rowCount,
            string siteList,
            string currencyList)
        {
            var jsonSelectSite = string.Empty;
            var jsonSelectCurrency = string.Empty;
            var jsonSelectStatus = string.Empty;
            DateTime? startDateParam = null;
            DateTime? endDateParam = null;

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
            totalPages = 0;

            if (startDate != null)
            {
                startDateParam = DateTimeOffset.Parse(startDate.ToString()).DateTime;
            }

            if (endDate != null)
            {
                endDateParam = DateTimeOffset.Parse(endDate.ToString()).DateTime;
            }

            var p = new DynamicParameters(new
            {
                SelectSite = jsonSelectSite,
                BirDate = birDate,
                BirMonth = birMonth,
                BirYear = birYear,
                UserName = userName,
                AccountName = accountName,
                LoginIp = loginIp,
                TransId = transId,
                TpUserName = tpUserName,
                Email = email,
                PhoneNo = phoneNo,
                StartDate = startDateParam,
                EndDate = endDateParam,
                PageSize = pageSize,
                SelectCurrency = jsonSelectCurrency,
                SiteList = siteList,
                AffId = affId,
                CurrncyList = currencyList,
                BankAccountNumber = bankAccountNumber,
                SelectStatus = jsonSelectStatus
            });
            p.Add("PageNumber", pageNumber, DbType.Int32, ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<CustInfoDetail>("Admin_CustInfoSearch_Get_V7", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(IEnumerable<int> selectSite,
            int? birDate,
            int? birMonth,
            int? birYear,
            string userName,
            string AccountName,
            string LoginIp,
            string transId,
            string TpUserName,
            string Email,
            string PhoneNo,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            string siteList,
            int? AffId,
            IEnumerable<int> selectCurrency,
            string BankAccountNumber,
            string currncyList,
            IEnumerable<int> selectstatus)
        {
            string SelectSite = null;
            string SelectCurrency = null;
            string SelectStatus = null;
            DateTime? startDateParam = null;
            DateTime? endDateParam = null;

            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            if (selectCurrency != null && selectCurrency.Any())
            {
                SelectCurrency = JsonConvert.SerializeObject(selectCurrency);
            }
            if (selectstatus != null)
            {
                if (selectstatus.ToList().Count == 0)
                {
                    var templist = new List<int>() { 1, 2, 3 };
                    SelectStatus = JsonConvert.SerializeObject(templist);
                }
                else
                {
                    SelectStatus = JsonConvert.SerializeObject(selectstatus);
                }
            }

            if (startDate != null)
            {
                startDateParam = DateTimeOffset.Parse(startDate.ToString()).DateTime;
            }

            if (endDate != null)
            {
                endDateParam = DateTimeOffset.Parse(endDate.ToString()).DateTime;
            }

            var p = new DynamicParameters(new { SelectSite, BirDate = birDate, BirMonth = birMonth, BirYear = birYear, UserName = userName, AccountName = AccountName, LoginIp = LoginIp, TpUserName = TpUserName, TransId = transId, Email = Email, PhoneNo = PhoneNo, StartDate = startDateParam, EndDate = endDateParam, SiteList = siteList, AffId, BankAccountNumber = BankAccountNumber, SelectCurrency = SelectCurrency, CurrncyList = currncyList, SelectStatus = SelectStatus });

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var custInfoDetailList = conn.Query<CustInfoDetailForExport>("Admin_CustInfoSearchForExport_Get_V5", p, commandType: CommandType.StoredProcedure);
            return custInfoDetailList;
        }

        public int GetVipLevel(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.ExecuteScalar<int>("VipLevel_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName, string siteList)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustInfoDetail>("Admin_CustInfoDetailSearch_Get_V7", new { CustId = custId, UserName = userName, SiteList = siteList }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public CustInfoDetail GetCustInfos(int? custId, string userName, int? siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustInfoDetail>("Admin_CustInfo_Get", new { CustId = custId, UserName = userName, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            return result;
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
        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustInfo_Set", new { CustId = custId, Level, FullName, Birthday_Y, Birthday_M, Birthday_D, PhoneNumber, EMail, Operator }, commandType: CommandType.StoredProcedure);
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

        public IEnumerable<CustNote> GetCustNotes(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<CustNote>("Admin_CustNotes_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public bool SetCustNotes(int custId, int noteId, string note, int type, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustNotes_Set", new { CustId = custId, NoteId = noteId, Note = note, Type = type, Operator = Operator }, commandType: CommandType.StoredProcedure) > 0;
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int CustId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<LoginLog>("Admin_LoginLogByCustV2_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
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
            return conn.Execute("Admin_ResetProfile_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
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
        public bool SetCustStatus(int custId, int status)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustStatus_Set", new { CustId = custId, Status = status }, commandType: CommandType.StoredProcedure) > 0;
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
        public IEnumerable<CustBankInfoLog> GetCustBankInfoLogList(int custId, int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustBankInfoLog>("Admin_BankInfoLogList_Get", new { CustId = custId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<CustCryptoWalletLog> GetCustCryptoWalletLogList(int custId, int siteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustCryptoWalletLog>("Admin_CryptoWalletLogList_Get", new { CustId = custId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
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

        #region Tag
        public int EditTagManagement(int id, string tagName, string tagColour, string description, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("Id", id);
            props.Add("TagName", tagName);
            props.Add("TagColour", tagColour);
            props.Add("Description", description);
            props.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_Tag_Set", props);
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

        public int SetTagStatus(int id, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("Id", id);
            props.Add("Operator", @operator);

            return _mainDbClient.Execute("Admin_TagStatus_Set", props);
        }

        public IEnumerable<TagData> GetAllTag()
        {
            var SearchResult = new GetTagDataResult();
            var props = new DynamicParameters();
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageNumber", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("PageSize", 0);
            props.Add("Status", 1);
            return _reportDbClient.Query<TagData>("Admin_Tag_Get", props);
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
            return _reportDbClient.Query<VipLevelModel>("Common_VipLevel_Get");
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
            
            var result = _reportDbClient.Query<RegLiveStatus>("Admin_CustRegLiveStatus_Get_V4", props).ToList();
            searchData.RegLiveStatusDatas = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return searchData;
        }

        public int SetRegLiveStatus(int custId, int status, int siteId, string @operator)
        {
            return _mainDbClient.Execute("Admin_RegLiveStatus_Set", new DynamicParameters(new { CustId = custId, Status = status, SiteId = siteId, Operator = @operator }));
        }

        public IEnumerable<RegLiveStatusNotes> GetRegLiveStatusNotes(int custId)
        {
            return _reportDbClient.Query<RegLiveStatusNotes>("Admin_CustRegLiveStatusNotes_Get_V2", new DynamicParameters(new { CustId = custId }));
        }

        public int AddRegLiveStatusNote(int custId, string note, string @operator)
        {
            return _mainDbClient.Execute("Admin_CustRegLiveStatusNotes_Ins", new DynamicParameters(new { CustId = custId, Note = string.IsNullOrWhiteSpace(note) ? "" : note, Operator = @operator }));
        }

        public int DeleteRegLiveStatusNote(int custId, int id)
        {
            return _mainDbClient.Execute("Admin_CustRegLiveStatusNotes_Del", new DynamicParameters(new { CustId = custId, Id = id }));
        }

        public int EditRegLiveStatusNote(int custid, int id, string note, string @operator)
        {
            return _mainDbClient.Execute("Admin_CustRegLiveStatusNotes_Set", new DynamicParameters(new { CustId = custid, Id = id, Note = note, Operator = @operator }));
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

        public async Task<CustomerForRisk> GetCustId(string userName, int siteId)
        {
            var props = new DynamicParameters();
            props.Add("CustId", null);
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
            return conn.QueryFirstOrDefault<CustProfileReq>("Admin_CustProfile_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
    }
}
