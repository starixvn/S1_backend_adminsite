using System.Data;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class CustInfoService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public CustInfoService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public Customer GetCustInfo(int custId,bool forceUseMainDb = false)
        {
            return forceUseMainDb
            ? _mainDbClient.QueryFirst<Customer>("Customer_Get", new DynamicParameters(new { CustId = custId }))
            : _reportDbClient.QueryFirst<Customer>("Customer_Get", new DynamicParameters(new { CustId = custId }));
        }

        public int GetPointLevel(int custId)
        {
            return _reportDbClient.QueryFirst<int>("Member_PointLevel_Get", new DynamicParameters(new { CustId = custId }));
        }
        
        public CustInfoDetail GetCustInfoDetail(int custId, string userName, string siteList)
        {
            var result = _reportDbClient.QueryFirst<CustInfoDetail>("Admin_CustInfoDetailSearch_Get_V12", new DynamicParameters(new { CustId = custId, UserName = userName, SiteList = siteList }));
            return result;
        }

        public CustInfoDetail GetCustInfos(int? custId, string userName, int? siteId)
        {
            return _mainDbClient.QueryFirst<CustInfoDetail>("Admin_CustInfo_Get", new DynamicParameters(new { CustId = custId, UserName = userName, SiteId = siteId}));
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

        public int SetCustStatusSwitch(int custId, int Op)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            parameter.Add("Op", Op);
            var result = _mainDbClient.Execute("Admin_CustStatusSwitch_Set", parameter);
            return result;
        }

        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, Dictionary<string, string> address, string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            parameter.Add("Level", Level);
            parameter.Add("FullName", FullName);
            parameter.Add("Birthday_Y", Birthday_Y);
            parameter.Add("Birthday_M", Birthday_M);
            parameter.Add("Birthday_D", Birthday_D);
            parameter.Add("PhoneNumber", PhoneNumber);
            parameter.Add("EMail", EMail);
            parameter.Add("Address", JsonConvert.SerializeObject(address));
            parameter.Add("Operator", Operator);
            var result = _mainDbClient.Execute("Admin_CustInfo_Set_V2", parameter);
            return result;
        }

        public CustPhotoId GetCustPhotoId(int CustId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", CustId);
            return _reportDbClient.QueryFirst<CustPhotoId>("Admin_CustPhotoId_Get", parameter);
        }

        public int SetCustPhotoId(int custId, int siteId, string picFileName, byte[] photoIdPic)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            parameter.Add("SiteId", siteId);
            parameter.Add("PicFileName", picFileName);
            parameter.Add("PhotoIdPic", photoIdPic);
            return _mainDbClient.Execute("Admin_CustPhotoId_Ins", parameter);
        }

        public bool DelCustPhotoId(int custId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            return _mainDbClient.Execute("Admin_CustPhotoId_Del", parameter) > 0;
        }

        public bool AddCustNote(int custId, string note, string @operator, MySqlTransaction trans = null, bool canEdit = false)
        {
            var p = new DynamicParameters(new
            {
                _CustId = custId,
                _Note = note,
                _Operator = @operator,
                _CanEdit = canEdit
            });
            const string spName = "Admin_CustNotes_Ins_V2";
            return trans != null
                ? _mainDbClient.Execute(spName, trans, p) > 0
                : _mainDbClient.Execute(spName, p) > 0;
        }

        public IEnumerable<CustNote> GetCustNotes(int custId,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
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
            var parameter = new DynamicParameters();

            parameter.Add("NoteId", noteId);
            parameter.Add("Note", note);
            parameter.Add("Type", type);
            parameter.Add("Operator", Operator);
            return _mainDbClient.Execute("Admin_CustNotes_Set_V2", parameter) > 0;
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

            var depositList = _reportDbClient.Query<LoginLog>("Admin_LoginLogByCust_Get_V3", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return depositList;
        }
        public bool SetResetPwd(int custId, string md5NewPassword)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            parameter.Add("Md5NewPassword", md5NewPassword);
            return _mainDbClient.Execute("Admin_ResetPwd_Set", parameter) > 0;
        }
        public bool ResetProfile(int custId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            return _mainDbClient.Execute("Admin_ResetProfile_Set_V2", parameter) > 0;
        }
        public bool ResetCustBankInfo(int custId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            return _mainDbClient.Execute("Admin_ResetCustBankInfo_Set", parameter) > 0;
        }
        public bool ResetCustUSDTInfo(int custId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            return _mainDbClient.Execute("Admin_ResetCustUSDTInfo_Set", parameter) > 0;
        }
        public bool SetCustStatus(int custId, int status, int taskId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("CustId", custId);
            parameter.Add("Status", status);
            parameter.Add("TaskId", taskId);
            return _mainDbClient.Execute("Admin_CustStatus_Set_V2", parameter) > 0;
        }
        public IEnumerable<CustBankInfoLog> GetCustBankInfoLogList(int custId,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_CustId", custId);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<CustBankInfoLog>("Admin_BankInfoLogList_Get_V3", parameter);

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
            var parameter = new DynamicParameters();

            parameter.Add("_CustId", custId);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<CustCryptoWalletLog>("Admin_CryptoWalletLogList_Get_V2", parameter);

            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }
        public IEnumerable<CustBonus> GetCustBonus(int custId)
        {
            var parameter = new DynamicParameters();

            parameter.Add("CustId", custId);
            return _reportDbClient.Query<CustBonus>("Admin_CustBonus_Get", parameter);
        }

        /// <summary>
        /// 刪除線上使用者資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteOnUserData(int custId)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId
                });

            var SpName = "Admin_OnUser_Del";
            return _mainDbClient.Execute(SpName, parameters);
        }

        /// <summary>
        /// 刪除使用者Line資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteLineProfile(int custId)
        {
            var parameters = new DynamicParameters(
                new
                {
                    _CustId = custId
                });

            var SpName = "Admin_CustLineProfile_Del";
            return _mainDbClient.Execute(SpName, parameters);
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
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var props = new DynamicParameters();
            props.Add("Id", input.After.Id);
            props.Add("TagName", input.After.TagName);
            props.Add("TagColour", input.After.TagColour);
            props.Add("Description", input.After.Description);
            props.Add("Operator", input.Operator);

            var success = _mainDbClient
                .Execute("Admin_Tag_Set", trans, props) > 0;

            if (success)
            {
                var isCreate = input.After.Id == 0;
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.TagManagement);
                logParam.Add("_ActionType", (int)(isCreate ? ActionType.Create : ActionType.Update));
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog(), DbType.String);
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;

                if (logSuccess)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }

            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
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
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var props = new DynamicParameters();
            props.Add("Id", input.After.Id);
            props.Add("Operator", input.Operator);

            var success = _mainDbClient
                .Execute("Admin_TagStatus_Set", trans, props) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.TagManagement);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;

                if (logSuccess)
                {
                    _mainDbClient.CommitAndDisposeConnection(conn, trans);
                    return true;
                }
            }

            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
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
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("EnablePromotion", enablePromotion);
            return _mainDbClient.Execute("Admin_CustInofoEnablePromotion_Set", props) > 0;
        }

        public async Task<bool> SetEnableRebateAsync(int custId, int status, MySqlTransaction trans)
        {
            return await _mainDbClient.ExecuteAsync("Admin_CustInfoEnableRebate_Set", trans, new DynamicParameters(new { _CustId = custId, _EnableRebate = status })) > 0;
        }

        public IEnumerable<VipLevelModel> GetAllSiteVipLevelList()
        {
            return _reportDbClient.Query<VipLevelModel>("Common_VipLevel_Get_V2");
        }

        public async Task<CustomerForRisk> GetCustId(int? custId, string userName, int? siteId)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("UserName", userName);
            props.Add("SiteId", siteId);
            return await _reportDbClient.QueryFirstAsync<CustomerForRisk>("Admin_CustInfo_Get", props);
        }

        public CustomerSwitchStatus GetCustSwitchStatus(int custId)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            return _reportDbClient.QueryFirst<CustomerSwitchStatus>("Admin_CustSwitchStatus_Get", props);
        }
        public CustProfileReq GetCustProfile(int custId)
        {
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            return _reportDbClient.QueryFirst<CustProfileReq>("Admin_CustProfile_Get_V3", props);
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

        public IEnumerable<CustEWalletLog> GetCustEWalletLogList(int custId,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var parameter = new DynamicParameters();

            parameter.Add("_CustId", custId);
            parameter.Add("_PageSize", pageSize);
            parameter.Add("_StartIndex", startIndex);
            parameter.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var data = _reportDbClient.Query<CustEWalletLog>("Admin_EWalletLogList_Get", parameter);

            totalPages = parameter.Get<int?>("_TotalPages") ?? 0;
            rowCount = parameter.Get<int?>("_RowCount") ?? 0;

            return data;
        }
        public async Task<bool> InsertLogAsync(int siteId, int custId, int currencyId, object beofreData, object afterData,
           AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater, MySqlTransaction trans)
        {

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CustId", custId);
            logParam.Add("_CurrencyId", currencyId);
            logParam.Add("_Menu", (int)adminAdjustmentLogMenu);
            logParam.Add("_ActionType", (int)actionType);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", beofreData.ToLog());
            logParam.Add("_AfterDetailInfo", afterData.ToLog());
            logParam.Add("_Operator", updater);

            return await _mainDbClient.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins_V2", trans, logParam) > 0;
        }

        public Task<IEnumerable<BankInfoDetail>> GetCustBankDetailAsync(int custId)
        {
            return _reportDbClient.QueryAsync<BankInfoDetail>("Admin_CustBankDetailByCustId_Get", new DynamicParameters(new { CustId = custId }));
        }
        public async Task<CustDashBoardTodayModel> GetCustDashBoardTodayAsync(int custId, int currencyId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_CurrencyId", currencyId);

            return await _reportDbClient.QueryFirstWithNoCacheAsync<CustDashBoardTodayModel>("Admin_CustDashBoard_Today_Get_V2", p);
        }

        public IEnumerable<BlockBankAcc> GetBankList(int custId)
        {
            return _reportDbClient.Query<BlockBankAcc>("Admin_CustBankDetail_Get", new DynamicParameters(new { CustId = custId }));
        }
        public IEnumerable<BlockEWalletAcc> GetEWalletList(int custId)
        {
            return _reportDbClient.Query<BlockEWalletAcc>("Admin_CustEWalletDetail_Get", new DynamicParameters(new { CustId = custId }));
        }
        public IEnumerable<CustLoginLog> GetIPLog(int custId, DateTime startDate,
            DateTime endDate)
        {
            return _reportDbClient.Query<CustLoginLog>("Admin_LoginLog_Get", new DynamicParameters(new { _CustId = custId, _StartDate = startDate, _EndDate = endDate }));
        }
        public DashboardWinLossGroup GetDashboardWinLossGroup(int custId, DateTimeOffset dateTime)
        {
            return _reportDbClient.Query<DashboardWinLossGroup>("Admin_DashboardWinLossGroup_Get_V2", new DynamicParameters(new { _CustId = custId, _Date = dateTime.DateTime })).FirstOrDefault();
        }
        public IEnumerable<WinLoseSummary> GetWinLoseSummary(int custId, DateTimeOffset dateTime)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_Date", dateTime.DateTime);
            return _reportDbClient.RetryQuery<WinLoseSummary>("Admin_WinLoseSummary_Get_V2", p);
        }

        public IEnumerable<WinLoseSummaryDetail> GetWinLoseSummaryDetail(int custId, DateTimeOffset dateTime, string GameTypes)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_Date", dateTime.DateTime);
            p.Add("_GameList", GameTypes);
            return _reportDbClient.RetryQuery<WinLoseSummaryDetail>("Admin_WinLoseSummaryDetail_Get_V4", p);            
        }

        public Task<IEnumerable<EWalletDetail>> GetCustEWalletDetailAsync(int custId)
        {
            return _reportDbClient.QueryAsync<EWalletDetail>("Admin_CustEWalletDetailByCustId_Get", new DynamicParameters(new { CustId = custId }));
        }

        internal async Task<IEnumerable<CustNote>> GetNoteById(int id)
        {
            return await _reportDbClient.QueryAsync<CustNote>("Admin_CustNotes_GetById", new DynamicParameters(new { Id = id }));
        }

        public IEnumerable<MemberExportDataDto> GetCustByExcel(ImportDataByExcelModel importDataByExcelModel)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_SiteId", importDataByExcelModel.SiteId);
            parameter.Add("_UserNameList", importDataByExcelModel.UserNameList);
            parameter.Add("_CustIdList", importDataByExcelModel.CustIdList);
            return _reportDbClient.Query<MemberExportDataDto>("Admin_CustinfoByImportData_Get", parameter);
        }

        public bool NewBatchCustInfo(int siteId, string memo, string author, ref int batchId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId, _Author = author, _Memo = memo });
            p.Add("BatchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("Admin_BatchCustInfo_Ins", p);
            batchId = p.Get<int>("BatchId");
            return result > 0;
        }

        public bool UpdateBatchCustInfoStatus(int status, int batchId)
        {
            var props = new DynamicParameters();
            props.Add("_Status", status);
            props.Add("_BatchId", batchId);
            return _mainDbClient.Execute("Admin_BatchCustInfo_Set", props) > 0;
        }

        public IEnumerable<BatchCustInfo> GetBatchCustInfo(int siteId)
        {
            return _reportDbClient.Query<BatchCustInfo>("Admin_BatchCustInfo_Get", new DynamicParameters(new { _SiteId = siteId, _Top = 10 }));
        }

        public Task<IEnumerable<USDTDetail>> GetCustCryptoWalletDetailAsync(int custId)
        {
            return _reportDbClient.QueryAsync<USDTDetail>("Service_CustCryptoWalletDetail_Get", new DynamicParameters(new { CustId = custId }));
        }

        public IEnumerable<CustCryptoWalletList> GetCryptoList(int custId)
        {
            return _reportDbClient.Query<CustCryptoWalletList>("Admin_CustCryptoWalletList_Get", new DynamicParameters(new { CustId = custId }));
        }
    }
}
