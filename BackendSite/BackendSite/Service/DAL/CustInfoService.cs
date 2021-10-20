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

namespace BackendSite.Service.DAL
{
    public class CustInfoService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public CustInfoService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }

        public Customer GetCustInfo(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<Customer>("Customer_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<CustInfoDetail> GetCustInfoList(IEnumerable<int> selectSite, int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, long? startDate, long? endDate, int pageSize, int? AffId, IEnumerable<int> selectCurrency, string bankAccountNumber, IEnumerable<int> selectstatus, ref int pageNumber, out int totalPages, out int rowCount, string siteList, string currncyList)
        {
            string SelectSite = null;
            string SelectCurrency = null;
            string SelectStatus = null;
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
            totalPages = 0;

            var p = new DynamicParameters(new { SelectSite, BirDate = birDate, BirMonth = birMonth, BirYear = birYear, UserName = userName, AccountName = AccountName, LoginIp = LoginIp, TransId = transId, TpUserName = TpUserName, Email = Email, PhoneNo = PhoneNo, StartDate = startDate, EndDate = endDate, PageSize = pageSize, SelectCurrency = SelectCurrency, SiteList = siteList, AffId, CurrncyList = currncyList, BankAccountNumber = bankAccountNumber, SelectStatus = SelectStatus });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<CustInfoDetail>("Admin_CustInfoSearchV5_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return result;
        }

        public IEnumerable<CustInfoDetailForExport> GetCustInfoExportList(IEnumerable<int> selectSite, int? birDate, int? birMonth, int? birYear, string userName, string AccountName, string LoginIp, string transId, string TpUserName, string Email, string PhoneNo, long? startDate, long? endDate, string siteList, int? AffId, IEnumerable<int> selectCurrency, string BankAccountNumber, string currncyList, IEnumerable<int> selectstatus)
        {
            string SelectSite = null;
            string SelectCurrency = null;
            string SelectStatus = null;
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
            var p = new DynamicParameters(new { SelectSite, BirDate = birDate, BirMonth = birMonth, BirYear = birYear, UserName = userName, AccountName = AccountName, LoginIp = LoginIp, TpUserName = TpUserName, TransId = transId, Email = Email, PhoneNo = PhoneNo, StartDate = startDate, EndDate = endDate, SiteList = siteList, AffId, BankAccountNumber = BankAccountNumber, SelectCurrency = SelectCurrency, CurrncyList = currncyList, SelectStatus = SelectStatus });

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var custInfoDetailList = conn.Query<CustInfoDetailForExport>("Admin_CustInfoSearchForExportV4_Get", p, commandType: CommandType.StoredProcedure);
            return custInfoDetailList;
        }

        public int GetVipLevel(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.ExecuteScalar<int>("VipLevel_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName, string siteList)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustInfoDetail>("Admin_CustInfoDetailSearchV6_Get", new { CustId = custId, UserName = userName, SiteList = siteList }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public CustInfoDetail GetCustInfos(int? custId, string userName, int? siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.QueryFirstOrDefault<CustInfoDetail>("Admin_CustInfo_Get", new { CustId = custId, UserName = userName, SiteId = siteId }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public LastDepositAndWithdrawalTime GetCustDepositAndWithdrawalTime(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.QueryFirstOrDefault<LastDepositAndWithdrawalTime>("Admin_CustInfoOther", new { CustId = custId }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public int SetCustStatusSwitch(int custId, int Op)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustStatusSwitch_Set", new { CustId = custId, Op }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var result = conn.Execute("Admin_CustInfo_Set", new { CustId = custId, Level, FullName, Birthday_Y, Birthday_M, Birthday_D, PhoneNumber, EMail, Operator }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public CustPhotoId GetCustPhotoId(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<CustPhotoId>("Admin_CustPhotoId_Get", new { CustId = CustId }, commandType: CommandType.StoredProcedure);
        }
        public int SetCustPhotoId(int custId, int siteId, string picFileName, byte[] photoIdPic)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Member_CustPhotoId_Int", new { CustId = custId, SiteId = siteId, PicFileName = picFileName, PhotoIdPic = photoIdPic }, commandType: CommandType.StoredProcedure);
        }
        public bool DelCustPhotoId(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustPhotoId_Del", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool AddCustNote(int custId, string Note, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustNotes_Ins", new { CustId = custId, Note = Note, Operator = Operator }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<CustNote> GetCustNotes(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<CustNote>("Admin_CustNotes_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public bool SetCustNotes(int custId, int noteId, string note, int type, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustNotes_Set", new { CustId = custId, NoteId = noteId, Note = note, Type = type, Operator = Operator }, commandType: CommandType.StoredProcedure) > 0;
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int CustId, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<LoginLog>("Admin_LoginLogByCustV2_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
        public bool SetResetPwd(int custId, string md5NewPassword)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetPwd_Set", new { CustId = custId, Md5NewPassword = md5NewPassword }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetProfile(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetProfile_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetCustBankInfo(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetCustBankInfo_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool ResetCustUSDTInfo(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_ResetCustUSDTInfo_Set", new { CustId = custId }, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetCustStatus(int custId, int status)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustStatus_Set", new { CustId = custId, Status = status }, commandType: CommandType.StoredProcedure) > 0;
        }
        public decimal GetWithdrawalAvailableBalance(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustBankInfoLog>("Admin_BankInfoLogList_Get", new { CustId = custId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<CustCryptoWalletLog> GetCustCryptoWalletLogList(int custId, int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustCryptoWalletLog>("Admin_CryptoWalletLogList_Get", new { CustId = custId, SiteId = siteId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<CustBonus> GetCustBonus(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustBonus>("Admin_CustBonus_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public class MutiSelect
        {
            public string label { get; set; }
            public int value { get; set; }
        }

        public TeleCSDData GetTeleCSD(string SiteIds, string Currency, long StartDate, long EndDate, int Deposit, int DormantUser, int CallOut, int PageNumber, int PageSize)
        {
            var SearchData = new TeleCSDData();
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            //var SiteIdsObject = JsonConvert.DeserializeObject<MutiSelect[]>(SiteIds);
            //var CurrencyObject = JsonConvert.DeserializeObject<MutiSelect[]>(Currency);

            //int siteidlength = SiteIdsObject == null ? 0 : SiteIdsObject.Length;
            //int Currencylength = CurrencyObject == null ? 0 : CurrencyObject.Length;
            //int[] SiteIdFilterArray = new int[siteidlength];
            //int[] CurrencyFilterArray = new int[Currencylength];
            //if (siteidlength > 0)
            //{
            //    for (var i = 0; i < SiteIdsObject.Length; i++)
            //    {
            //        SiteIdFilterArray[i] = SiteIdsObject[i].value;
            //    }
            //}
            //if (Currencylength > 0)
            //{
            //    for (var i = 0; i < CurrencyObject.Length; i++)
            //    {
            //        CurrencyFilterArray[i] = CurrencyObject[i].value;
            //    }
            //}
            //string SiteIdFilter = JsonConvert.SerializeObject(SiteIdFilterArray);
            //string CurrencyFilter = JsonConvert.SerializeObject(CurrencyFilterArray);
            //int ActStatus = int.Parse(Deposit);
            //int DayInactive = int.Parse(DormantUser);
            //int CallOutStatus = int.Parse(CallOut);

            props.Add("PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", PageSize);

            props.Add("SelectSite", SiteIds);
            props.Add("CurrencyList", Currency);
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("ActStatus", Deposit);
            props.Add("DayInactive", DormantUser);
            props.Add("CallOutStatus", CallOut);
            var result = conn.Query<CustTele>("Admin_CustTeleSearchV3_Get", props, commandType: CommandType.StoredProcedure).ToList();

            result.ForEach(x =>
            {
                if (x.UpdateAt != null)
                {
                    var UpdateAtArray = x.UpdateAt.Split(',');
                    var OperatorArray = x.Operator.Split(',');
                    x.UpdateRemark = new List<string>();
                    for (var i = 0; i < UpdateAtArray.Length; i++)
                    {
                        x.UpdateRemark.Add($"{UpdateAtArray[i]}-{OperatorArray[i]}");
                    }
                    x.CallOutStatus = UpdateAtArray.Length;
                    if (UpdateAtArray.Length == 3)
                    {
                        x.CanUpdate = false;
                    }
                }
            });

            SearchData.CustTeleList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchData;
        }

        public int UpdateCustomerTele(int siteId, int custId, string Operator)
        {
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));

            props.Add("SiteId", siteId);
            props.Add("CustId", custId);
            props.Add("Operator", Operator);

            return conn.Execute("Admin_CustTele_Ins", props, commandType: CommandType.StoredProcedure);
        }

        public List<CustTeleExport> GetTeleCSDExportData(string SiteIds, string Currency, long StartDate, long EndDate, int Deposit, int DormantUser, int CallOut)
        {
            var result = new List<CustTeleExport>();
            var props = new DynamicParameters();
            //var SiteIdsObject = JsonConvert.DeserializeObject<MutiSelect[]>(SiteIds);
            //var CurrencyObject = JsonConvert.DeserializeObject<MutiSelect[]>(Currency);
            //int siteidlength = SiteIdsObject == null ? 0 : SiteIdsObject.Length;
            //int Currencylength = CurrencyObject == null ? 0 : CurrencyObject.Length;
            //int[] SiteIdFilterArray = new int[siteidlength];
            //int[] CurrencyFilterArray = new int[Currencylength];
            //if (siteidlength > 0)
            //{
            //    for (var i = 0; i < SiteIdsObject.Length; i++)
            //    {
            //        SiteIdFilterArray[i] = SiteIdsObject[i].value;
            //    }
            //}
            //if (Currencylength > 0)
            //{
            //    for (var i = 0; i < CurrencyObject.Length; i++)
            //    {
            //        CurrencyFilterArray[i] = CurrencyObject[i].value;
            //    }
            //}
            //string SiteIdFilter = JsonConvert.SerializeObject(SiteIdFilterArray);
            //string CurrencyFilter = JsonConvert.SerializeObject(CurrencyFilterArray);
            //int ActStatus = int.Parse(Deposit);
            //int DayInactive = int.Parse(DormantUser);
            //int CallOutStatus = int.Parse(CallOut);

            props.Add("SelectSite", SiteIds);
            props.Add("CurrencyList", Currency);
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("ActStatus", Deposit);
            props.Add("DayInactive", DormantUser);
            props.Add("CallOutStatus", CallOut);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var tempresult = conn.Query<CustTeleExportTemp>("Admin_CustTeleExportV3_Get", props, commandType: CommandType.StoredProcedure).ToList();

            tempresult.ForEach(x =>
            {
                var firstcall = string.Empty;
                var secondcall = string.Empty;
                var thirdcall = string.Empty;
                if (x.UpdateAt != null && x.Operator != null)
                {
                    var UpdateAtArray = x.UpdateAt.Split(',');
                    var OperatorArray = x.Operator.Split(',');
                    for (var i = 0; i < UpdateAtArray.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                firstcall = $"{UpdateAtArray[i]}-{OperatorArray[i]}";
                                break;
                            case 1:
                                secondcall = $"{UpdateAtArray[i]}-{OperatorArray[i]}";
                                break;
                            case 2:
                                thirdcall = $"{UpdateAtArray[i]}-{OperatorArray[i]}";
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
                    FirstTimeCall = firstcall,
                    SecondTimeCall = secondcall,
                    ThirdTimeCall = thirdcall
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId
                });

            var SpName = "Admin_OnUser_Del";
            return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
        }

        #region Tag
        public int EditTagMamagement(int id, string tagName, string tagColour, string description, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("Id", id);
            props.Add("TagName", tagName);
            props.Add("TagColour", tagColour);
            props.Add("Description", description);
            props.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_Tag_Set", props);
        }

        public GetTagDataResult GetTagData(int status,int pageNumber,int pageSize)
        {
            var SearchResult = new GetTagDataResult();
            var props = new DynamicParameters();
            props.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", pageSize);
            props.Add("Status", status);

            var result = _reportDbClient.Query<TagData>("Admin_TagV2_Get", props);

            SearchResult.TagDatas = result.ToList();
            SearchResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchResult;
        }

        public int SetTagStatus(int Id, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("Id", Id);
            props.Add("Operator", Operator);

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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustInofoEnablePromotion_Set", new { CustId = custId, EnablePromotion = enablePromotion }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<VipLevelModel> GetAllSiteVipLevelList()
        {
            return _reportDbClient.Query<VipLevelModel>("Common_VipLevel_Get");
        }

        public RegLiveStatusResult GetRegLiveStatusData(IEnumerable<int> selectSite, int currency, int depositStatus, int status, string username, int pagenumber, int pagesize, int sort, long StartDate, long EndDate, string AffiliatesCode, int AgentCodeType)
        {
            //CreditSite
            var SearchData = new RegLiveStatusResult();
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            List<string> CreditSite = new List<string>() { "Nova88Ref01", "Nova88Ref02", "Nova88Ref03", "Nova88Ref04", "Nova88Ref05", "Nova88Ref06", "Nova88Ref07", "Nova88Ref08", "Goal123Ref01", "BwingRef01" };
            props.Add("SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(username) ? null : username);
            props.Add("DepositStatus", depositStatus);
            props.Add("Status", status);
            props.Add("Sort", sort);
            props.Add("PageSize", pagesize);
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("AffCode", string.IsNullOrWhiteSpace(AffiliatesCode) ? null : AffiliatesCode);
            props.Add("CreditSite", JsonConvert.SerializeObject(CreditSite));
            props.Add("AgentCodeType", AgentCodeType);
            props.Add("PageNumber", pagenumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<RegLiveStatus>("Admin_CustRegLiveStatusV3_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.RegLiveStatusDatas = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchData;
        }

        public int SetRegLiveStatus(int custid,int status, int siteid, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_RegLiveStatus_Set", new { CustId = custid, Status = status, SiteId= siteid, Operator= Operator }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<RegLiveStatusNotes> GetRegLiveStatusNotes(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<RegLiveStatusNotes>("Admin_CustRegLiveStatusNotes_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public int AddRegLiveStatusNote(int custid, string note, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustRegLiveStatusNotes_Ins", new { CustId = custid, Note = string.IsNullOrWhiteSpace(note) ? "" : note, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }

        public int DeleteRegLiveStatusNote(int custid,int id)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustRegLiveStatusNotes_Del", new { CustId = custid, Id = id }, commandType: CommandType.StoredProcedure);
        }

        public int EditRegLiveStatusNote(int custid, int id, string note, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_CustRegLiveStatusNotes_Set", new { CustId = custid, Id = id, Note = note, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<RegLiveStatus> GetExportRegLiveStatus(string SelectSite, int Currency, int DepositStatus, int Status, string User, int Sort, long StartDate, long EndDate, string AffiliatesCode, int AgentCodeType)
        {
            IEnumerable<RegLiveStatus> result = null;
            List<string> CreditSite = new List<string>() { "Nova88Ref01", "Nova88Ref02", "Nova88Ref03", "Nova88Ref04", "Nova88Ref05", "Nova88Ref06", "Nova88Ref07", "Nova88Ref08", "Goal123Ref01", "BwingRef01" };
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Admin_CustRegLiveStatusExportV3_Get";
            var props = new DynamicParameters();
            props.Add("SelectSite", SelectSite);
            props.Add("Currency", Currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(User) ? null : User);
            props.Add("DepositStatus", DepositStatus);
            props.Add("Status", Status);
            props.Add("Sort", Sort);
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("AffCode", string.IsNullOrWhiteSpace(AffiliatesCode) ? null : AffiliatesCode);
            props.Add("CreditSite", JsonConvert.SerializeObject(CreditSite));
            props.Add("AgentCodeType", AgentCodeType);

            result = conn.Query<RegLiveStatus>(SpName, props, commandType: CommandType.StoredProcedure);

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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<CustomerSwitchStatus>("Admin_CustSwitchStatus_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public CustProfileReq  GetCustProfile(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<CustProfileReq>("Admin_CustProfile_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
    }
}
