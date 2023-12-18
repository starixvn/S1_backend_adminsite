using System.Text.RegularExpressions;
using System.Transactions;
using Amazon.S3.Model;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;
using OfficeOpenXml;
using Tag = BackendSite.Service.Model.Tag;

namespace BackendSite.Service.BLL
{
    public class CustInfoBLL
    {
        private readonly CustInfoService _custInfoService;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly TpBLL _tpBll;
        private readonly WalletService _walletService;
        private readonly Tools _tools;
        private readonly CodeGenerator _codeGenerator;
        private readonly PromotionService _promotionService;
        private readonly RollOverService _rolloverService;
        private readonly BonusInfoService _bonusInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _bonusApiUrl;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly ILogger<CustInfoBLL> _logger;
        private readonly MySqlClient _mainDbClient;
        private readonly BetListService _betListBll;
        private readonly IEnumerable<TpGameTypeOption> _tpGameTypeList;
        private readonly ICacheHelper _redisHelper;

        private readonly WinLostReportService _winLostReportService;
        private readonly RebateManageService _rebateManageService;
        private readonly ExcelBLL _excelBll;
        private readonly AWSS3Helper _s3Helper;
        private readonly string _backOfficeS3Url;
        private readonly string _backOfficeS3BucketName;
        private readonly string _environment;
        private readonly TagBLL _tagll;
        public CustInfoBLL(IServiceProvider provider)
        {
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _walletService = provider.GetRequiredService<WalletService>();
            _tools = provider.GetRequiredService<Tools>();
            _codeGenerator = provider.GetRequiredService<CodeGenerator>();
            _promotionService = provider.GetRequiredService<PromotionService>();
            _rolloverService = provider.GetRequiredService<RollOverService>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _bonusApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").BonusService;
            _backOfficeS3Url = configBll.GetConfigValue("AWSBackOfficeBucketUrl");
            _backOfficeS3BucketName = configBll.GetConfigValue("AWSBackOfficeBucketName");

            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();
            _logger = provider.GetRequiredService<ILogger<CustInfoBLL>>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _betListBll = provider.GetRequiredService<BetListService>();
            _tpGameTypeList = provider.GetRequiredService<IEnumerable<TpGameTypeOption>>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _winLostReportService = provider.GetRequiredService<WinLostReportService>();
            _rebateManageService = provider.GetRequiredService<RebateManageService>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
            var env = provider.GetRequiredService<IWebHostEnvironment>();
            _environment = env.IsProduction() || env.EnvironmentName == "Release" ? "PRD" : "DEV";
            _tagll = provider.GetRequiredService<TagBLL>();
        }

        public Customer GetCustInfoWithCache(int custId)
        {
            return _redisHelper.TryGetOrCreate($"CustInfo:{custId}", TimeSpan.FromMinutes(1),
                () => _custInfoService.GetCustInfo(custId),
                RedisDataBase.Shared);
        }

        public int GetPointLevelWithCache(int custId)
        {
            return _redisHelper.TryGetOrCreate($"PointLevel:{custId}", TimeSpan.FromMinutes(1),
                () => _custInfoService.GetPointLevel(custId),
                RedisDataBase.Shared);
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
            out int rowCount)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUserV2().SelectMany(x => new[] { x.SiteId });
            var currencyList = new List<int>();
            _serverInfoBll.GetSiteListByLoginUserV2().ToList().ForEach(x =>
            {
                currencyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });
            var wTransId = IsWithdrawalInfoTransId(transId) ? transId : null;
            var dTransId = IsDepositInfoTransId(transId) ? transId : null;
            var tTransId = IsTpTransferInfoTransId(transId) ? transId : null;

            var custInfoList = _custInfoService.GetCustInfoList(selectSite, birDate, birMonth, birYear, userName, accountName, loginIp, wTransId, dTransId, tTransId, tpUserName, email, phoneNo, startDate, endDate, tagStartDate, tagEndDate, statusStartDate, statusEndDate, pageSize, affCode, selectCurrency, bankAccountNumber, selectStatus, selectTag, ref pageNumber, out totalPages, out rowCount, JsonConvert.SerializeObject(siteList), JsonConvert.SerializeObject(currencyList.Select(x => x).Distinct().ToList())).ToList();
            if (custInfoList.Count > 0)
            {
                custInfoList.ForEach(x =>
                {
                    x.ITagList = string.IsNullOrWhiteSpace(x.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(x.TagList);
                });
            }
            return custInfoList;
        }

        public IEnumerable<T> GetCustInfoExportList<T>(IEnumerable<int> selectSite,
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
            DateTime? startDate,
            DateTime? endDate,
            DateTime? tagStartDate,
            DateTime? tagEndDate,
            DateTime? statusStartDate,
            DateTime? statusEndDate,
            string affCode,
            IEnumerable<int> selectCurrency,
            string bankAccountNumber,
            IEnumerable<int> selectStatus,
            IEnumerable<int> selectTag)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUserV2().SelectMany(x => new int[] { x.SiteId });

            var currencyList = new List<int>();
            _serverInfoBll.GetSiteListByLoginUserV2().ToList().ForEach(x =>
            {
                currencyList.AddRange(JsonConvert.DeserializeObject<List<int>>(x.CurrencyList));
            });

            var wTransId = IsWithdrawalInfoTransId(transId) ? transId : null;
            var dTransId = IsDepositInfoTransId(transId) ? transId : null;
            var tTransId = IsTpTransferInfoTransId(transId) ? transId : null;

            var result = _custInfoService.GetCustInfoExportList<T>(selectSite, birDate, birMonth, birYear, userName, accountName, loginIp, wTransId, dTransId, tTransId, tpUserName, email, phoneNo, startDate, endDate, tagStartDate, tagEndDate, statusStartDate, statusEndDate, JsonConvert.SerializeObject(siteList), affCode, selectCurrency, bankAccountNumber, JsonConvert.SerializeObject(currencyList.Select(x => x).Distinct().ToList()), selectStatus, selectTag);


            return result;
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUserV2()?.SelectMany(x => new int[] { x.SiteId });
            return siteList is null ? null : GetCustInfoDetail(custId, userName, siteList);
        }

        public CustInfoDetail GetCustInfoDetail(int custId, string userName, int siteId)
        {
            var siteList = new int[] { siteId };
            return GetCustInfoDetail(custId, userName, siteList);
        }

        private CustInfoDetail GetCustInfoDetail(int custId, string userName, IEnumerable<int> siteList)
        {
            var custInfoDetail = _custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.tpWallet = _thirdPartyService.GetTpWallet(custId);
                custInfoDetail.promotions = _promotionService.GetPromotionListByCustId(custInfoDetail.CustId);
                if (custInfoDetail.promotions != null)
                {
                    foreach (var promotion in custInfoDetail.promotions)
                    {
                        if (promotion.BonusInfoStatus is (int)BonusInfoStatusEnum.Running or (int)BonusInfoStatusEnum.TrasferOut)
                        {
                            promotion.custRollOver = _rolloverService.GetCustRollOver(custInfoDetail.CustId, promotion.BonusCode);
                            if (promotion.BonusInfoStatus == (int)BonusInfoStatusEnum.TrasferOut)
                            {
                                promotion.BonusInfoStatus = (int)BonusInfoStatusEnum.Running;
                            }
                        }
                        else
                        {
                            promotion.custRollOver = new CustRollOver() { RollOver = 0, WinOver = 0, TurnOver = 0 };
                        };
                    }
                }
                if (custInfoDetail.CustPhoneReward.HasValue || custInfoDetail.CustEmailReward.HasValue)
                {
                    custInfoDetail.VerificationReward = GetRewardMessage(custInfoDetail);
                }
                custInfoDetail.ITagList = string.IsNullOrWhiteSpace(custInfoDetail.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(custInfoDetail.TagList);
                custInfoDetail.CustLineProfile = _custInfoService.GetCustLineProfile(custId, null);
                custInfoDetail.CustBonusSum = _bonusInfoService.GetCustBonusSum(custInfoDetail.SiteId, custId);
                custInfoDetail.CustRebateSum = _bonusInfoService.GetCustRebateSum(custInfoDetail.SiteId, custId);
                custInfoDetail.CustRewardSum = _bonusInfoService.GetCustRewardSum(custInfoDetail.SiteId, custId);
            }

            return custInfoDetail;
        }

        public CustInfoDetail GetCustInfoDetails(int? custId, string userName, int? siteId)
        {
            return _custInfoService.GetCustInfos(custId, userName, siteId);
        }

        public bool SendMessageBatch(BatchPersonalMsg req)
        {
            return _custInfoService.SendMessageBatch(req);
        }

        public decimal GetBalance(int custId)
        {
            return _walletService.GetBalance(custId);
        }

        public Task<decimal> GetBalanceAsync(int custId)
        {
            return _walletService.GetBalanceAsync(custId);
        }
        public int SetCustStatusSwitch(int custId, int Op)
        {
            return _custInfoService.SetCustStatusSwitch(custId, Op);
        }
        public int SetCustInfo(int custId, int? Level, string FullName, int? Birthday_Y, int? Birthday_M, int? Birthday_D, string PhoneNumber, string EMail, Dictionary<string, string> address, string Operator)
        {
            return _custInfoService.SetCustInfo(custId, Level, FullName, Birthday_Y, Birthday_M, Birthday_D, PhoneNumber, EMail, address, Operator);
        }
        public IEnumerable<LoginLog> GetLoginLogByCust(int CustId, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            return _custInfoService.GetLoginLogByCust(CustId, startDate, endDate, Math.Abs(pageSize), (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), out totalPages, out rowCount);
        }
        public bool SetResetPwd(int CustId, out string NewPassword)
        {
            _codeGenerator.Maximum = 8;
            _codeGenerator.Minimum = 6;
            _codeGenerator.RepeatCharacters = true;
            NewPassword = _codeGenerator.Generate();
            var md5NewPassword = _tools.MD5Encrypt(NewPassword);
            return _custInfoService.SetResetPwd(CustId, md5NewPassword);
        }
        public bool ResetProfile(int CustId)
        {
            return _custInfoService.ResetProfile(CustId);
        }
        public bool ResetCustBankInfo(int CustId)
        {
            return _custInfoService.ResetCustBankInfo(CustId);
        }
        public bool ResetCustUSDTInfo(int CustId)
        {
            return _custInfoService.ResetCustUSDTInfo(CustId);
        }
        public async Task<(int Result, int CustId)> CancelBonusByBonusId(int bonusId, string @operator)
        {
            var statusList = new List<int>
            {
                (int) BonusInfoStatusEnum.Initial,
                (int) BonusInfoStatusEnum.Running,
                (int) BonusInfoStatusEnum.TrasferOut,
            };
            return await _bonusInfoService.CancelBonusByBonusId(bonusId, JsonConvert.SerializeObject(statusList), @operator);
        }

        public List<CustTurnoverRes> GetCustTurnover(int CustId,
            IEnumerable<int> GameTypes,
            DateTime StartTime,
            DateTime EndTime)
        {
            var result = _betListBll
                .GetTpBetRecord(CustId, StartTime, EndTime)
                .Where(i => GameTypes.Contains(i.GameType))
                .Join(_tpGameTypeList,
                    tpBetRecord => tpBetRecord.GameType,
                    tpGameType => tpGameType.Type,
                    (tpBetRecord, tpGameType) => new CustTurnover
                    {
                        UserName = tpBetRecord.UserName,
                        GameType = tpBetRecord.GameType,
                        GameName = tpGameType.TypeName,
                        Stake = tpBetRecord.ValidBet ?? tpBetRecord.Stake,
                        WinlostAmount = tpBetRecord.WinlostAmount,
                        SettlementType = tpBetRecord.SettlementType,
                        TpUserName = tpBetRecord.TpUserName,
                    })
                .GroupBy(i => i.GameType)
                .Select(i => new CustTurnoverRes
                {
                    UserName = i.First().UserName,
                    GameType = i.Key,
                    GameName = i.First().GameName,
                    StakeSettled = i.Where(i => i.SettlementType == (int)SettlementType.SettledForValid).Sum(i => i.Stake),
                    StakeNotRollover = i.Where(i => i.SettlementType == (int)SettlementType.SettledForValidNotRollover).Sum(i => i.Stake),
                    WinlostAmount = i.Sum(i => i.WinlostAmount),
                    TpUserName = i.First().TpUserName
                })
                .ToList();

            return result;
        }

        public IEnumerable<CustBankInfoLog> GetCustBankInfoLogList(int custId,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustBankInfoLogList(custId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }
        public IEnumerable<CustCryptoWalletLog> GetCustCryptoWalletLogList(int custId,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustCryptoWalletLogList(custId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount); ;
        }
        public IEnumerable<CustBonus> GetCustBonus(int custId)
        {
            return _custInfoService.GetCustBonus(custId);
        }

        public PromotionListData GetPromotionListByCustIdAndDate(int custId, DateTimeOffset StartDate, DateTimeOffset EndDate, int PageNumber, int PageSize)
        {
            var result = _promotionService.GetPromotionListByCustIdAndDate(custId, StartDate, EndDate, PageNumber, PageSize);
            if (result.PromotionList != null)
            {
                foreach (var promotion in result.PromotionList)
                {
                    if (promotion.BonusInfoStatus == 1 || promotion.BonusInfoStatus == 5)
                    {
                        promotion.custRollOver = _rolloverService.GetCustRollOver(custId, promotion.BonusCode);
                        if (promotion.BonusInfoStatus == 5)
                        {
                            promotion.BonusInfoStatus = 1;
                        }
                    }
                    else
                    {
                        promotion.custRollOver = new CustRollOver() { RollOver = 0, WinOver = 0, TurnOver = 0 };
                    };

                }
            }
            return result;
        }


        public async Task<PromotionResponseWrapper> GetPromotionListByCustIdAndDateAsync(CustIdWithDateRangeAndPageRequest request)
        {
            var result = await _promotionService.GetPromotionListByCustIdAndDateAsync(request);

            if (result.PromotionList != null)
            {
                foreach (var promotion in result.PromotionList)
                {
                    if (promotion.BonusInfoStatus == 1 || promotion.BonusInfoStatus == 5)
                    {
                        promotion.custRollOver = _rolloverService.GetCustRollOver(request.CustId, promotion.BonusCode);
                        if (promotion.BonusInfoStatus == 5)
                        {
                            promotion.BonusInfoStatus = 1;
                        }
                    }
                    else
                    {
                        promotion.custRollOver = new CustRollOver() { RollOver = 0, WinOver = 0, TurnOver = 0 };
                    };

                    promotion.DepositInfo = CheckDepositInfoFormatter(promotion.DepositInfo);
                }
            }
            return result;
        }

        private string CheckDepositInfoFormatter(string depositInfo)
        {
            if (string.IsNullOrEmpty(depositInfo) || depositInfo.Contains("[")) return depositInfo;
            return $"[{depositInfo}]";
        }

        #region

        #endregion

        #region Customer Photo

        public CustPhotoId GetCustPhotoId(int custId)
        {
            return _custInfoService.GetCustPhotoId(custId);
        }
        public int SetCustPhotoId(int custId, int siteId, string picFileName, byte[] photoIdPic)
        {
            return _custInfoService.SetCustPhotoId(custId, siteId, picFileName, photoIdPic);
        }
        public bool DelCustPhotoId(int custId)
        {
            return _custInfoService.DelCustPhotoId(custId);
        }

        #endregion

        #region Customer Note

        public async Task<bool> AddCustNote(int custId, string note, string @operator, bool canEdit = false)
        {
            var custSimpleInfo = await GetCustId(custId, null, null);
            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            if (!_custInfoService.AddCustNote(custId, note, @operator, canEdit: canEdit))
            {
                _logger.LogError($"SetNote: Add Note Fail. CustId:{custId} Note:{note} CanEdit:{canEdit} Operator:{@operator}.");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }

            if (canEdit)
            {
                var afterData = new { Note = note, CanEdit = false };
                if (!await _custInfoService.InsertLogAsync(custSimpleInfo.SiteId, custSimpleInfo.CustId, custSimpleInfo.CurrencyId, null, afterData,
                    AdminAdjustmentLogMenuEnum.SetCustInfoNote, ActionType.Create, @operator, trans))
                {
                    _logger.LogError($"SetNote: Insert Operate Log Fail. CustId:{custId} Note:{note} CanEdit:{canEdit} Operator:{@operator}.");
                    await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                    return false;
                }
            }

            await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
            return true;
        }
        public IEnumerable<CustNote> GetCustNotes(int custId,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustNotes(custId,
                 Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }
        public async Task<bool> SetCustNotes(int noteId, string note, int type, string Operator)
        {
            var noteBeforeInfo = await GetNoteById(noteId);
            var custSimpleInfo = await GetCustId(noteBeforeInfo.First().CustId, null, null);
            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            if (!_custInfoService.SetCustNotes(noteId, note, type, Operator))
            {
                _logger.LogError($"SetNote: Set Note Fail. CustId:{noteBeforeInfo.First().CustId} NoteId:{noteId} Note:{note} Type:{type} Operator:{Operator}.");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
            var actionType = type == (int)SetCustNoteTypeEnum.Delete ? ActionType.Delete : ActionType.Update;
            var beforeData = new { NoteId = noteId, Note = noteBeforeInfo.First().Note };
            var afterData = new { NoteId = noteId, Note = note };

            if (!await _custInfoService.InsertLogAsync(custSimpleInfo.SiteId, custSimpleInfo.CustId, custSimpleInfo.CurrencyId, beforeData,
                actionType == ActionType.Delete ? null : afterData,
                AdminAdjustmentLogMenuEnum.SetCustInfoNote, actionType, Operator, trans))
            {
                _logger.LogError($"SetNote: Insert Operate Log Fail. CustId:{noteBeforeInfo.First().CustId} NoteId:{noteId} Note:{note} Type:{type} Operator:{Operator}..");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }

            await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
            return true;
        }

        public async Task<IEnumerable<CustNote>> GetNoteById(int id) => await _custInfoService.GetNoteById(id);

        #endregion

        #region Customer Status Update

        public bool SetCustStatus(string menu, int custId, int status, string reason, string action, string user)
        {
            var result = false;
            var mainInfo = new Dictionary<string, dynamic>();
            var noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", action);
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using var scope = new TransactionScope();

            #region Update Status

            result = _custInfoService.SetCustStatus(custId, status, (int)CustTaskType.LastStatusUpdateTime);

            if (!result)
            {
                scope.Dispose();

                return false;
            };

            if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Update, mainInfo, user) <= 0)
            {
                scope.Dispose();

                return false;
            }

            #endregion

            if (result)
            {
                #region Insert Reason For Update Status

                if (!_custInfoService.AddCustNote(custId, action + " Reason : " + reason, user))
                {
                    scope.Dispose();

                    return false;
                }

                if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Create, noteInfo, user) <= 0)
                {
                    scope.Dispose();

                    return false;
                }

                #endregion
            }

            scope.Complete();

            return result;

        }

        /// <summary>
        /// 刪除在線使用者資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int DeleteOnUserData(string menu, int custId, string reason, string user)
        {
            var result = 0;
            var mainInfo = new Dictionary<string, dynamic>();
            var noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", "Kick Out");
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using var scope = new TransactionScope();

            #region Kick Out User

            result = _custInfoService.DeleteOnUserData(custId);

            if (result <= 0)
            {
                scope.Dispose();

                return 0;
            };

            if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Delete, mainInfo, user) <= 0)
            {
                scope.Dispose();

                return 0;
            }

            #endregion

            if (result > 0)
            {
                #region Insert Reason For Kick Out

                if (!_custInfoService.AddCustNote(custId, "Kick Out Reason : " + reason, user))
                {
                    scope.Dispose();

                    return 0;
                }

                if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Create, noteInfo, user) <= 0)
                {
                    scope.Dispose();

                    return 0;
                }

                #endregion
            }

            scope.Complete();

            return result;
        }

        /// <summary>
        /// 刪除使用者Line資料
        /// </summary>
        /// <param name="custId">客戶ID</param>
        /// <returns></returns>
        public int LineUnbind(string menu, int custId, string user)
        {
            var result = 0;
            var mainInfo = new Dictionary<string, dynamic>();
            var noteInfo = new Dictionary<string, dynamic>();
            mainInfo.Add("MenuName", "Customer Info");
            mainInfo.Add("Action", "Line Inbind");
            noteInfo.Add("MenuName", "Customer Info");
            noteInfo.Add("Action", "Add Note");

            using var scope = new TransactionScope();

            #region Kick Out User

            result = _custInfoService.DeleteLineProfile(custId);

            if (result <= 0)
            {
                scope.Dispose();

                return 0;
            };

            if (_serverInfoBll.AdminOperateLog(null, custId, menu, ActionType.Delete, mainInfo, user) <= 0)
            {
                scope.Dispose();

                return 0;
            }

            #endregion

            scope.Complete();

            return result;
        }

        #endregion

        #region TagMamagement
        public bool EditTagManagement(ParamsWithLog<EditTagManagement> input)
        {
            return _custInfoService.EditTagManagement(input);
        }

        public GetTagDataResult GetTagData(int status, int pageNumber, int pageSize)
        {
            return _custInfoService.GetTagData(status, pageNumber, pageSize);
        }

        public bool SetTagStatus(ParamsWithLog<SetTagStatus> input)
        {
            return _custInfoService.SetTagStatus(input);
        }

        public IEnumerable<TagData> GetAllTag()
        {
            return _custInfoService.GetAllTag();
        }

        public int SetCustTag(int custid, IEnumerable<int> SelectTag, string Operator)
        {
            return _custInfoService.SetCustTag(custid, SelectTag);
        }
        public CustInfoDetail GetCustData(int custId, string userName)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUserV2().SelectMany(x => new int[] { x.SiteId });
            var custInfoDetail = _custInfoService.GetCustInfoDetail(custId, userName, JsonConvert.SerializeObject(siteList));
            if (custInfoDetail != null)
            {
                custInfoDetail.ITagList = string.IsNullOrWhiteSpace(custInfoDetail.TagList) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(custInfoDetail.TagList);
            }

            return custInfoDetail;
        }
        #endregion

        public int SetEnableDisablePromotion(int custId, int enablePromotion, string Operator)
        {
            var result = 1;

            using var scope = new TransactionScope();
            if (!_custInfoService.SetEnableDisablePromotion(custId, enablePromotion))
            {
                scope.Dispose();

                return 0;
            }
            if (!_custInfoService.AddCustNote(custId, enablePromotion == 0 ? "Disable Promotion" : "Enable Promotion", Operator))
            {
                scope.Dispose();

                return 0;
            }

            scope.Complete();
            return result;
        }

        public async Task<bool> SetEnableRebateAsync(int custId, EnableRebateEnum status, string Operator)
        {
            var custSimpleInfo = await GetCustId(custId, null, null);
            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            if (!await _custInfoService.SetEnableRebateAsync(custId, (int)status, trans))
            {
                _logger.LogError($"SetEnableRebate: Update EnableRebate Fail. CustId:{custId} EnableRebate:{status.ToString()}.");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
            if (!_custInfoService.AddCustNote(custId, status == EnableRebateEnum.Disable ? "Disable Rebate Payout" : "Enable Rebate Payout", Operator, trans))
            {
                _logger.LogError($"SetEnableRebate: Insert CustNote Fail. CustId:{custId} EnableRebate:{status.ToString()}.");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
            var beforeData = new { UserName = custSimpleInfo.UserName, EnableRebate = status == EnableRebateEnum.Disable ? EnableRebateEnum.Enable : EnableRebateEnum.Disable };
            var afterData = new { UserName = custSimpleInfo.UserName, EnableRebate = status };

            if (!await _custInfoService.InsertLogAsync(custSimpleInfo.SiteId, custSimpleInfo.CustId, custSimpleInfo.CurrencyId, beforeData, afterData,
                AdminAdjustmentLogMenuEnum.SetCustInfoEnableRebate, ActionType.Update, Operator, trans))
            {
                _logger.LogError($"SetEnableRebate: Insert Operate Log Fail. CustId:{custId} EnableRebate:{status.ToString()}.");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }

            await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
            return true;
        }

        public IEnumerable<VipLevelModel> GetAllSiteVipLevelList()
        {
            return _custInfoService.GetAllSiteVipLevelList();
        }

        public async Task<CustomerForRisk> GetCustId(int? custId, string userName, int? siteId)
        {
            return await _custInfoService.GetCustId(custId, userName, siteId);
        }

        public Task<IEnumerable<CustomerInfo>> GetCustInfosAsync(int siteId, IEnumerable<string> userNames)
        {
            var strUserNames = JsonConvert.SerializeObject(userNames);

            return _custInfoService.GetCustInfosAsync(siteId, strUserNames);
        }

        public CustomerSwitchStatus GetCustSwitchStatus(int custId)
        {
            return _custInfoService.GetCustSwitchStatus(custId);
        }

        public CustProfileReq GetCustProfile(int custId)
        {
            return _custInfoService.GetCustProfile(custId);
        }

        private string GetRewardMessage(CustInfoDetail custInfoDetail)
        {
            var rewardMessage = "";
            switch (custInfoDetail.VerificationType)
            {
                case 0:
                    {
                        if (custInfoDetail.CustPhoneReward.HasValue)
                            rewardMessage += $"Phone: {custInfoDetail.CustPhoneReward.Value:0.00} ";
                        if (custInfoDetail.CustEmailReward.HasValue)
                            rewardMessage += $"Email : {custInfoDetail.CustEmailReward.Value:0.00}";
                        break;
                    }
                case 1:
                    rewardMessage = $"Phone & Email: {custInfoDetail.CustPhoneReward:0.00}";
                    break;
            }

            return rewardMessage;
        }
        public ApiResult SetVerificationReward(int custId, int OTPType)
        {
            var apiResult = new ApiResult();
            var myUrl = $"{_bonusApiUrl}SetVerificationReward";

            var verificationReq = new SetVerificationReq()
            {
                CustId = custId,
                OTPType = OTPType
            };

            apiResult = _courierAdapter.MyPostAsync<ApiResult, SetVerificationReq>(myUrl, verificationReq).Result;

            return apiResult;
        }

        public ApiResult CheckRewardCondination(int custId, int OTPType)
        {
            var apiResult = new ApiResult();
            var myUrl = $"{_bonusApiUrl}CheckRewardCondination";
            var verificationReq = new SetVerificationReq()
            {
                CustId = custId,
                OTPType = OTPType
            };
            var data = _courierAdapter.MyPostAsync<ApiResult, SetVerificationReq>(myUrl, verificationReq).Result;

            return data;
        }

        public async Task<CustomerWhoWaitToSend> GetCustInfoByCust(int siteId, int? custId, string userName)
        {
            return await _custInfoService.GetCustInfoByCust(siteId, custId, userName);
        }

        public async Task<IEnumerable<CustomerWhoWaitToSend>> GetCustInfoBySite(int siteId, int currencyId, int status)
        {
            return await _custInfoService.GetCustInfoBySite(siteId, currencyId, status);
        }

        public WithdrawalLimitLogData GetWithdrawalLimitLog(int custId, DateTimeOffset StartDate, DateTimeOffset EndDate, int PageNumber, int PageSize)
        {
            return _custInfoService.GetWithdrawalLimitLog(custId, StartDate, EndDate, PageNumber, PageSize);
        }

        public async Task<CustInfoDetailForExport> GetTransferWalletAsync(CustInfoDetailForExport custInfoDetailForExport)
        {
            var result = custInfoDetailForExport;
            var transferWallet = await _tpBll.GetTransferTpWalletWithCacheAsync(custInfoDetailForExport.CustId);
            var ptwallet = transferWallet.ToList().Find(y => y.TpId == (int)TpEnum.PT);
            if (ptwallet != null)
            {
                result.PTWallet = ptwallet.Balance;
            }
            return result;
        }
        public async Task<MemberExportData> GetTransferWalletAsync(MemberExportData custInfoDetailForExport)
        {
            var result = custInfoDetailForExport;
            var transferWallet = await _tpBll.GetTransferTpWalletWithCacheAsync(custInfoDetailForExport.CustId);
            var ptwallet = transferWallet.ToList().Find(y => y.TpId == (int)TpEnum.PT);
            if (ptwallet != null)
            {
                result.PTWallet = ptwallet.Balance;
            }
            return result;
        }

        public async Task<IEnumerable<TpWallet>> GetCustTPWalletAsync(int custId)
        {
            return await _tpBll.GetTransferTpWalletWithCacheAsync(custId);
        }

        public Task<IEnumerable<BankInfoDetail>> GetCustBankDetailAsync(int custId)
        {
            return _custInfoService.GetCustBankDetailAsync(custId);
        }

        public Task<IEnumerable<EWalletDetail>> GetCustEWalletDetailAsync(int custId)
        {
            return _custInfoService.GetCustEWalletDetailAsync(custId);
        }

        public Task<IEnumerable<USDTDetail>> GetCustCryptoInfoListAsync(int custId)
        {
            return _custInfoService.GetCustCryptoWalletDetailAsync(custId);
        }

        private bool IsWithdrawalInfoTransId(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId))
            {
                return false;
            }
            if (transId.Length < 3 || transId.Substring(0, 1) != "W")
            {
                return false;
            }
            return true;
        }
        private bool IsDepositInfoTransId(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId))
            {
                return false;
            }
            if (transId.Length < 3 || transId.Substring(0, 1) != "D")
            {
                return false;
            }
            return true;
        }
        private bool IsTpTransferInfoTransId(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId))
            {
                return false;
            }
            if (transId.Length < 3 || (transId.Substring(0, 1) != "T" && transId.Substring(0, 2) != "AT" && transId.Substring(0, 2) != "BT"))
            {
                return false;
            }
            return true;
        }
        public IEnumerable<CustEWalletLog> GetCustEWalletLogList(int custId,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            return _custInfoService.GetCustEWalletLogList(custId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount); ;
        }

        public async Task<IEnumerable<ThirdPartyAccountResponse>> GetThirdPartyAccountAsync(ThirdPartyAccountRequest request)
        {
            var key = $"TpAccountName_{request.CustId}";

            var thirdPartyAccount = await _redisHelper.TryGetOrCreateAsync(key, TimeSpan.FromSeconds(30),
                async () => await _thirdPartyService.GetTpCustInfoListByCustId(request.CustId));

            return thirdPartyAccount
                .Select(r => new ThirdPartyAccountResponse()
                {
                    TpName = r.TpName,
                    TpUserName = r.TpUserName
                });
        }


        public async Task<TpAdjustmentLogWrapper> GetCustAdjustmentTpLogV2(GetCustTpAdjustmentLogRequestV2 request, int siteId)
        {
            var dbParams = new GetCustTpAdjustmentLogParamsV2()
            {
                CustId = request.CustId,
                TpId = request.TpId,
                SiteId = siteId,
                Direction = request.Direction,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                StartIndex = (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize)
            };
            var queryResult = await _thirdPartyService.GetCustTpAdjustmentLogV2(dbParams);

            foreach (var data in queryResult.Data)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }

            return queryResult;
        }

        public async Task<CustDashBoardTodayResponse> GetCustDashBoardTodayAsync(int custId, int currencyId)
        {
            var data = await _custInfoService.GetCustDashBoardTodayAsync(custId, currencyId);
            _logger.LogInformation($"GetCustDashBoardTodayAsyncResult：{JsonConvert.SerializeObject(data)} CsutId:{custId} and currencyId:{currencyId}");

            return new CustDashBoardTodayResponse()
            {
                TodayDeposit = data.TodayDeposit,
                TodayWithdrawal = data.TodayWithdrawal,
                TodayWinLoss = data.TodayWinLoss,
                TodayBetting = data.TodayBetting,
                TodayBonus = data.TodayBonus,
                TodayReward = data.TodayReferralAmount + data.TodayReferralFreeCredit + data.TodayPhoneReward +
                              data.TodayEmailReward + data.TodayDraw + data.TodayUpgradeAmount
            };
        }

        public IEnumerable<BlockBankAcc> GetBankList(int custId)
        {
            return _custInfoService.GetBankList(custId);
        }

        public IEnumerable<BlockEWalletAcc> GetEWalletList(int custId)
        {
            return _custInfoService.GetEWalletList(custId);
        }

        public IEnumerable<CustLoginLog> GetIPLog(int custId, DateTime startDate,
            DateTime endDate)
        {
            var result = _custInfoService.GetIPLog(custId, startDate, endDate);
            if (result != null)
            {
                result.ToList().ForEach(x =>
                {
                    x.IsBlock = !string.IsNullOrWhiteSpace(x.UpdateOperator);
                });
            }
            return result;
        }

        public IEnumerable<GameType> GetGameTypeList(IEnumerable<int> TPCategoryList)
        {
            var siteIds = _serverInfoBll.GetSiteList().Select(x => x.SiteId);
            var AllGameType = _winLostReportService.GetAllGameType(System.Text.Json.JsonSerializer.Serialize(siteIds), 0).ToList();
            return AllGameType.Where(x => TPCategoryList.Contains(x.CategoryId)).ToList();
        }

        public DashboardWinLossGroupData GetDashboardWinLossGroup(int custId)
        {
            var Date = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");
            var tempDashboardWinLossGroup = _custInfoService.GetDashboardWinLossGroup(custId, Convert.ToDateTime(Date));

            return new DashboardWinLossGroupData()
            {
                Winnings = tempDashboardWinLossGroup.Winnings,
                Losses = tempDashboardWinLossGroup.Losses,
                WinLoss = tempDashboardWinLossGroup.Winnings + tempDashboardWinLossGroup.Losses,
                OutstandingAmount = tempDashboardWinLossGroup.OutstandingAmount,
                ValidBet = tempDashboardWinLossGroup.TpBetRecordStake + tempDashboardWinLossGroup.RelayStake
            };
        }

        public IEnumerable<WinLoseSummary> GetWinLoseSummary(int custId)
        {
            var Date = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");
            var winLoseSummary = _custInfoService.GetWinLoseSummary(custId, Convert.ToDateTime(Date)).ToList();
            List<WinLoseSummary> result = winLoseSummary.GroupBy(x => x.CategoryId).Select(y => new WinLoseSummary
            {
                WinlostAmount = y.Sum(z => z.WinlostAmount),
                Name = y.First().Name,
                CategoryId = y.Key
            }).ToList();
            var tPCategoryList = _rebateManageService.GetTPCategoryList().ToList();

            tPCategoryList.ForEach(x =>
            {
                if (!result.Any(y => y.CategoryId == x.CategoryId))
                {
                    result.Add(new WinLoseSummary()
                    {
                        WinlostAmount = 0,
                        Name = x.CategoryName,
                        CategoryId = x.CategoryId
                    });
                }
            });

            return result.OrderBy(x => x.CategoryId);
        }
        public IEnumerable<WinLoseSummaryDetailResult> GetWinLoseSummaryDetail(int custId, string GameTypes)
        {
            var Date = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");
            var winLoseSummaryDetail = _custInfoService.GetWinLoseSummaryDetail(custId, Convert.ToDateTime(Date), GameTypes).ToList();
            List<WinLoseSummaryDetailResult> result = winLoseSummaryDetail.GroupBy(x => x.GameType).Select(y => new WinLoseSummaryDetailResult
            {
                WinlostAmount = y.Sum(z => z.WinlostAmount),
                GameName = y.First().GameName,
                GameType = y.Key,
                StakeSettled = y.Sum(z => z.SettlementType == 1 ? z.Stake : 0),
                StakeNotRollover = y.Sum(z => z.SettlementType == 3 ? z.Stake : 0),
                TpUserName = y.First().TpUserName
            }).ToList();

            return result;
        }

        public bool ImportByExcel(int siteId, IFormFile excel, string memo, string author)
        {
            List<string> userNameList = new List<string>();
            List<int> custIdList = new List<int>();
            var batchId = 0;

            if (excel.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        StringComparison.OrdinalIgnoreCase) ||
                    excel.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
                    excel.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                var excelPkg = new ExcelPackage(excel.OpenReadStream());
                var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
                var isLastRow = false;
                var RowId = 2; // 因為有標題列，所以從第2列開始讀起

                do // 讀取資料，直到讀到空白列為止
                {
                    var custUsername = sheet.Cells[RowId, 1].Text.Trim();
                    var sabaId = sheet.Cells[RowId, 2].Text.Trim();
                    if (sabaId.IndexOf("_") > -1)
                    {
                        sabaId = sabaId.Substring(sabaId.LastIndexOf("_") + 1,
                            sabaId.Length - sabaId.LastIndexOf("_") - 1);
                    }
                    else if (sabaId.IndexOf("x") > -1)
                    {
                        sabaId = sabaId.Substring(sabaId.LastIndexOf("x") + 1,
                            sabaId.Length - sabaId.LastIndexOf("x") - 1);
                    }
                    var m = Regex.Match(sabaId, @"\d+");
                    var _custId = int.TryParse(m.Value, out var custId) ? custId : (int?)null;

                    if (!string.IsNullOrWhiteSpace(custUsername))
                    {
                        userNameList.Add(custUsername);
                    }
                    if (_custId != null)
                    {
                        custIdList.Add(custId);
                    }
                    if (string.IsNullOrEmpty(custUsername) && _custId == null)
                    {
                        isLastRow = true;
                    }
                    RowId += 1;
                } while (!isLastRow);

                //新增BatchCustInfo
                _custInfoService.NewBatchCustInfo(siteId, memo, author, ref batchId);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var custInfo = _custInfoService.GetCustByExcel(new ImportDataByExcelModel() { SiteId = siteId, CustIdList = JsonConvert.SerializeObject(custIdList.Distinct()), UserNameList = JsonConvert.SerializeObject(userNameList.Distinct()) });
                        var allTagList = await _tagll.GetAllTag();
                        var data = custInfo.Select(a => new MemberExportData()
                        {
                            SiteName = a.SiteName,
                            UserName = a.UserName,
                            CustId = a.CustId,
                            Currency = a.IsoCode,
                            BirthDate = a.BirthDate,
                            BirthMonth = a.BirthMonth,
                            BirthYear = a.BirthYear,
                            IsTest = a.IsTest,
                            IsKyc = a.IsKyc,
                            IsRisk = a.IsRisk,
                            SeamlessWallet = a.SeamlessWallet,
                            BonusWallet = a.BonusWallet,
                            Email = a.Email,
                            RealName = a.RealName,
                            CountryCode = a.CountryCode,
                            AreaCode = a.AreaCode,
                            PhoneNo = a.PhoneNo,
                            LevelName = a.LevelName,
                            Agent = a.Agent,
                            RegTime = new DateTimeOffset(a.RegTime, TimeSpan.Zero),
                            Status = ((CustomerStatusEnum)a.Status).ToString(),
                            SabaId = a.SabaId,
                            SignupUrl = a.SignupUrl,
                            AffiliateId = a.AffiliateId,
                            AffiliateCode = a.AffiliateCode,
                            LastActivityIp = a.LastActivityIp,
                            Tag = GetTag(allTagList, a.TagList)
                        });
                        var fileBytes = _excelBll.ExportV2(data);
                        var stream = new MemoryStream(fileBytes);
                        var site = (SiteEnum)siteId;
                        var key = $"{_environment}/{site}/CustInfo/BatchMemberInfoReport_{batchId}.xlsx";
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = _backOfficeS3BucketName,
                            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            InputStream = stream,
                            Key = key
                        };

                        var result = await _s3Helper.PutObjectAsync(putRequest);

                        if (result.HttpStatusCode.IsSuccess())
                        {
                            _custInfoService.UpdateBatchCustInfoStatus((int)BatchCustInfoStatus.Finished, batchId);
                        }
                        else
                        {
                            _custInfoService.UpdateBatchCustInfoStatus((int)BatchCustInfoStatus.Failed, batchId);
                        }
                    }
                    catch
                    {
                        _custInfoService.UpdateBatchCustInfoStatus((int)BatchCustInfoStatus.Failed, batchId);
                    }
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<BatchCustInfo> GetCustinfoByImportData(int siteId)
        {
            var batchCustInfo = _custInfoService.GetBatchCustInfo(siteId).ToList();

            batchCustInfo.ForEach(x =>
            {
                x.StatusName = ((BatchCustInfoStatus)x.Status).ToString();
                x.Url = $"{_backOfficeS3Url}{_environment}/{(SiteEnum)siteId}/CustInfo/BatchMemberInfoReport_{x.BatchId}.xlsx";
            });

            return batchCustInfo;
        }

        private string GetTag(IEnumerable<Tag> allTagList, string tagList)
        {
            List<string> temp = new List<string>();
            if (string.IsNullOrWhiteSpace(tagList))
            {
                return string.Empty;
            }

            JsonConvert.DeserializeObject<List<int>>(tagList).ToList().ForEach(x =>
            {
                var tagName = allTagList.FirstOrDefault(y => y.Id == x)?.TagName;
                if (!string.IsNullOrWhiteSpace(tagName))
                {
                    temp.Add(tagName);
                }
            });

            return string.Join(",", temp);
        }

        public IEnumerable<CustCryptoWalletList> GetCryptoList(int custId)
        {
            return _custInfoService.GetCryptoList(custId);
        }

        public CustTpGameTypeSettingTypeEnum ConvertCustTpGameTypeSettingType(CustTpGameTypeSettingGetType type)
        {
            switch (type)
            {
                case CustTpGameTypeSettingGetType.IsShow:
                case CustTpGameTypeSettingGetType.NoShow:
                default:
                    return CustTpGameTypeSettingTypeEnum.NoShow;
                case CustTpGameTypeSettingGetType.IsPlay:
                case CustTpGameTypeSettingGetType.NoPlay:
                    return CustTpGameTypeSettingTypeEnum.NoPlay;
            }
        }
    }
}
