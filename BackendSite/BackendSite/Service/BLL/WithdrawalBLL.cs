using System.Text.RegularExpressions;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.WithdrawalBankAccounting;
using Newtonsoft.Json;

using static BackendSite.Controllers.ApiV2.Finance.Withdrawal.WithdrawalController;

namespace BackendSite.Service.BLL
{
    public class WithdrawalBLL
    {
        #region Fields

        private readonly WithdrawalService _withdrawalService;
        private readonly ServerInfoService _serverInfoService;
        private readonly WithdrawalLimitService _withdrawalLimitService;
        private readonly ReportService _reportService;
        private readonly WithdrawalBankAccountingService _withdrawalBankAccountingService;
        private readonly CustInfoBLL _custInfoBLL;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBLL;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly MySqlClient _mainDbClient;
        private readonly Common _common;
        private readonly ILogger<WithdrawalBLL> _logger;
        private readonly string _payApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly PaymentManageService _paymentManageService;

        private readonly Dictionary<int, bool> _isConvert = new Dictionary<int, bool>()
        {
            { 51, false },
            { 15, false }
        };

        #endregion

        #region Constructors

        public WithdrawalBLL(IServiceProvider provider)
        {

            _withdrawalService = provider.GetRequiredService<WithdrawalService>();
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
            _reportService = provider.GetRequiredService<ReportService>();
            _withdrawalBankAccountingService = provider.GetRequiredService<WithdrawalBankAccountingService>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportBLL = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _logger = provider.GetRequiredService<ILogger<WithdrawalBLL>>();
            _common = provider.GetRequiredService<Common>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _payApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").PayService;
            
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _paymentManageService = provider.GetRequiredService<PaymentManageService>();
        }

        #endregion

        #region Public Methods

        public IEnumerable<WithdrawalRequest> GetWithdrawalRequestList(IEnumerable<int> selectSite,
            IEnumerable<int> withdrawalType,
            DateTime startDate,
            DateTime endDate,
            WithdrawalDateTypeEnum dateType,
            string withdrawalId,
            IEnumerable<string> userNames,
            IEnumerable<int> currencyId,
            IEnumerable<int> status,
            IEnumerable<int> levelStatus,
            int pageSize,
            decimal startAmount,
            decimal endAmount,
            int pageNumber,
            WithdrawalMethodEnum withdrawalMethod,
            IEnumerable<string> selectBankCode,
            IEnumerable<int> selectCalCode,
            int agent,
            out int totalPages,
            out int rowCount,
            ref List<TotalAmount> currencyAmount)
        {
            return _withdrawalService.GetWithdrawalRequestList(selectSite,
                withdrawalType,
                startDate,
                endDate,
                dateType,
                withdrawalId,
                userNames,
                currencyId,
                status,
                levelStatus,
                Math.Abs(pageSize) * (Math.Abs(pageNumber) - 1),
                pageSize,
                startAmount,
                endAmount,
                withdrawalMethod,
                selectBankCode,
                selectCalCode,
                agent,
                out totalPages,
                out rowCount,
                ref currencyAmount);
        }

        public WithdrawalRequest GetWithdrawalRequest(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            return _withdrawalService.GetWithdrawalRequest(withdrawalId);
        }

        public IEnumerable<WithdrawalComments> GetWithdrawalRequestComment(string transId)
        {
            var withdrawalRequest = GetWithdrawalRequest(transId);
            var results = _withdrawalService.GetWithdrawalRequestComment(withdrawalRequest.WithdrawalId,
                withdrawalRequest.SiteId);
            return results;
        }


        public IEnumerable<WithdrawalRequest> GetWithdrawalListByCust(int custId,
            string transId,
            int? status,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount,
            out decimal totalAmount,
            out decimal totalApproveAmount,
            out int totalApproveRow)
        {
            return _withdrawalService.GetWithdrawalListByCust(custId,
                transId,
                status,
                startDate,
                endDate,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount,
                out totalAmount,
                out totalApproveAmount,
                out totalApproveRow);
        }

        public IEnumerable<WithdrawalInfoForExport> GetWithdrawalExportList(
            string selectSite,
            string withdrawalType,
            DateTime startDate,
            DateTime endDate,
            WithdrawalDateTypeEnum dateType,
            string withdrawalId,
            string userName,
            string currencyId,
            string status,
            string levelStatus,
            decimal startAmount,
            decimal endAmount,
            string selectBankCode,
            string selectCalCode,
            int agent,
            WithdrawalMethodEnum withdrawalMethod)
        {
            return _withdrawalService.GetWithdrawalExportList(
                selectSite,
                withdrawalType,
                startDate,
                endDate,
                dateType,
                withdrawalId,
                userName,
                currencyId,
                status,
                levelStatus,
                startAmount,
                endAmount,
                selectBankCode,
                selectCalCode,
                agent,
                withdrawalMethod);
        }

        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(IEnumerable<int> siteId,
            IEnumerable<int> currencyId,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount) =>
            _withdrawalLimitService.GetWithdrawalLimitInfos(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(currencyId),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(int siteId, int currencyId)
            => _withdrawalLimitService.GetWithdrawalLimitInfos(siteId, currencyId);

        public bool AddWithdrawalLimit(ParamsWithLog<SetWithdrawalLimitInfo> input)
        {
            if (input.After.TotalWithdrawalMax < 0
                || input.After.WithdrawalMin < 0
                || input.After.Times < 0)
            {
                return false;
            }

            return _withdrawalLimitService.AddWithdrawalLimitInfo(input);
        }

        public bool EditWithdrawalLimit(ParamsWithLog<SetWithdrawalLimitInfo> input)
        {
            if (input.After.WithdrawalMax < 0
                || input.After.WithdrawalMin < 0
                || input.After.Times < 0)
            {
                return false;
            }

            return _withdrawalLimitService.EditWithdrawalLimitInfo(input);
        }

        public bool EditWithdrawalLimit(ParamsWithLog<IEnumerable<SetWithdrawalLimitInfo>> input)
        {
            return _withdrawalLimitService.EditWithdrawalLimitInfo(input);
        }

        public bool DeleteWithdrawalLimit(ParamsWithLog<SetWithdrawalLimitInfo> input)
        {
            var currentUpperPoint =
                _withdrawalLimitService.GetWithdrawalLimitInfo(input.SiteId,
                    input.CurrencyId, input.Before.VipLevel + 1);

            if (currentUpperPoint != null)
                return false;

            return _withdrawalLimitService.DeleteWithdrawalLimitInfo(input);
        }

        public IEnumerable<WithdrawalKYC> GetWithdrawalKYCList(int siteId)
        {
            return _withdrawalService.GetWithdrawalKYC(siteId);
        }

        public bool EditWithdrawalKYCList(ParamsWithLog<WithdrawalKYCAmount> input)
        {
            return _withdrawalService.EditWithdrawalKYC(input);
        }

        public List<WithdrawalData> GetWithdrawalLimitData(int siteId)
        {
            //IEnumerable<CurrencyDataV2> CurrencyData = null;
            IEnumerable<ServerInfo> siteDataList = null;
            var result = new List<WithdrawalData>();
            siteDataList = _serverInfoService.GetSiteList();
            var currencyData =
                siteDataList.Where(x => x.SiteId == siteId)?.FirstOrDefault()?.ICurrencyList.ToList();
            var currencyList =
                _reportService.GetCurrencyList().ToDictionary(x => x.CurrencyId, x => x.IsoCode);
            var withdrawalMinInfo = _withdrawalService.GetWithdrawalLimitData(siteId).ToList();
            currencyData.ForEach(x =>
            {
                var target = withdrawalMinInfo.Find(y => y.CurrencyId == x);
                result.Add(new WithdrawalData()
                {
                    HasDetail = target != null,
                    CurrencyId = x,
                    CurrencyName = currencyList[x],
                    SiteId = siteId,
                    WithdrawalMin = target?.WithdrawalMin ?? 0
                });
            });
            return result;
        }

        public bool InsertWithdrawalLimit(int siteId, int currencyId, int withdrawalMin, string updater)
        {
            var withdrawalMinInfo = _withdrawalService.GetWithdrawalLimitData(siteId).ToList();
            if (withdrawalMinInfo.Exists(x => x.CurrencyId == currencyId))
            {
                return true;
            }

            return _withdrawalService.InsertWithdrawalLimit(siteId, currencyId, withdrawalMin, updater);
        }

        public bool UpdateWithdrawalLimit(int siteId, int currencyId, int withdrawalMin, string updater)
        {
            return _withdrawalService.UpdateWithdrawalLimit(siteId, currencyId, withdrawalMin, updater);
        }

        public bool DeleteWithdrawalLimit(int siteId, int currencyId)
        {
            return _withdrawalService.DeleteWithdrawalLimit(siteId, currencyId);
        }

        public bool UpdateWithdrawalPicCount(string transId, int count)
        {
            return _withdrawalService.UpdateWithdrawalPicCount(transId, count);
        }

        public BankSlipPic GetWithdrawalPicCount(string transId)
        {
            return _withdrawalService.GetWithdrawalPicCount(transId);
        }

        public WithdrawalTimeData WithdrawalTimeReportGet(string selectSite,
            string withdrawalId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string selectStatus,
            string currencyIds,
            int custId,
            int pageNumber,
            int pageSize,
            out List<TotalAmount> currencyTotalAmount)
        {
            var result = _withdrawalService.WithdrawalTimeReportGet(selectSite, withdrawalId, startDate, endDate, selectStatus, currencyIds, custId, pageNumber, pageSize, out currencyTotalAmount);

            result?.WithdrawalTimeList.ForEach(x =>
            {
                var ts1 = new TimeSpan(x.CreateTime.Ticks);
                var timeSpanOfFlowEndTime = new TimeSpan(x.FlowEndTime.Ticks);
                var ts = ts1.Subtract(timeSpanOfFlowEndTime).Duration();
                x.CompleteTime = GetTimeString(ts);
                if (x.VerifyTime > DateTime.Now.AddYears(-1000))
                {
                    var ts3 = new TimeSpan(x.VerifyTime.Value.Ticks);
                    var tsv = ts1.Subtract(ts3).Duration();
                    if (x.Status == 6)
                    {
                        var tsa = ts3.Subtract(timeSpanOfFlowEndTime).Duration();
                        x.ToApprovedTime = GetTimeString(tsa);
                    }

                    x.ToVerifyTime = GetTimeString(tsv);
                }
            });

            return result;
        }

        public async Task<IEnumerable<WithdrawalTimeExportData>> WithdrawalTimeExportGet(string selectSite, string withdrawalId,
            DateTime startDate, DateTime endDate, string status, string currencyId, string memberId)
        {
            var exportDataToExcel = new List<WithdrawalTimeExportData>();

            int custId = 0;
            if (!string.IsNullOrWhiteSpace(memberId))
            {
                var custSimpleInfo = await _custInfoBLL.GetCustId(null, memberId, JsonConvert.DeserializeObject<List<int>>(selectSite).FirstOrDefault());
                if (custSimpleInfo != null)
                    custId = custSimpleInfo.CustId;
                else
                    return exportDataToExcel;
            }

            var result =
                _withdrawalService.GetExportData(selectSite, withdrawalId, startDate, endDate, status, currencyId, custId);

            if (result != null)
            {
                result.ForEach(x =>
                {
                    var ts1 = new TimeSpan(x.CreateTime.Ticks);
                    var timeSpanOfFlowEndTime = new TimeSpan(x.FlowEndTime.Ticks);
                    var ts = ts1.Subtract(timeSpanOfFlowEndTime).Duration();
                    var tempToVerifyTime = "";
                    var tempToProcessTime = "";
                    var tempToApprovedTime = "";
                    if (x.VerifyTime > DateTime.Now.AddYears(-1000))
                    {
                        var ts2 = new TimeSpan(x.VerifyTime.Value.Ticks);
                        if (x.ProcessTime > DateTime.Now.AddYears(-1000))
                        {
                            var ts3 = new TimeSpan(x.ProcessTime.Value.Ticks);
                            var tsp = ts2.Subtract(ts3).Duration();
                            tempToProcessTime = GetTimeString(tsp);
                        }

                        var tsv = ts1.Subtract(ts2).Duration();
                        if (x.Status == 6)
                        {
                            var tsa = ts2.Subtract(timeSpanOfFlowEndTime).Duration();
                            tempToApprovedTime = GetTimeString(tsa);
                        }

                        tempToVerifyTime = GetTimeString(tsv);
                    }

                    DateTime? flowEndTime = null;
                    //Not Flow End.
                    if (x.Status == (int)WithdrawalStatusEnum.Cancelled
                    || x.Status == (int)WithdrawalStatusEnum.Approved
                    || x.Status == (int)WithdrawalStatusEnum.Rejected)
                    {
                        flowEndTime = x.FlowEndTime.DateTime;
                    }

                    exportDataToExcel.Add(new WithdrawalTimeExportData
                    {
                        SiteName = x.SiteName,
                        WithdrawalId = x.WithdrawalId,
                        WithdrawalType = x.WithdrawalTypeName,
                        UserName = x.UserName,
                        Currency = x.IsoCode,
                        Status = x.StatusName,
                        OperateBy = x.Operator,
                        CreateTime = x.CreateTime,
                        UpdateTime = x.UpdateTime,
                        FlowEndTime = flowEndTime,
                        ToVerifyTime = tempToVerifyTime,
                        ToApprovedTime = tempToApprovedTime,
                        ToProcessTime = tempToProcessTime,
                        CompleteTime = GetTimeString(ts),
                        VerifiedBy = x.VerifiedBy,
                        ApprovedBy = x.ApprovedBy,
                        ProcessedBy = x.ProcessedBy
                    });
                });
            }

            return exportDataToExcel;
        }

        public IEnumerable<USDTWithdrawalLimitInfoManage> GetUSDTWithdrawalLimitInfos(IEnumerable<int> siteId,
            IEnumerable<int> currencyId, string network,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            var datas = _withdrawalLimitService.GetUSDTWithdrawalLimitInfos(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(currencyId),
                network,
                 (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            datas.ToList().ForEach(x =>
            {
                x.NetworkType = (int)(NetworkTypeEnum)Enum.Parse(typeof(NetworkTypeEnum), x.Network);
            });
            return datas;
        }

        public bool AddUSDTWithdrawalLimit(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage, string updater)
        {
            if (USDTwithdrawalLimitInfoManage.WithdrawalMax < 0 || USDTwithdrawalLimitInfoManage.WithdrawalMin < 0)
            {
                return false;
            }

            return _withdrawalLimitService.AddUSDTWithdrawalLimitInfo(USDTwithdrawalLimitInfoManage, updater);
        }

        public int DeleteUSDTWithdrawalLimit(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage, string Operator)
        {
            var currentUpperPoint = _withdrawalLimitService.GetUSDTWithdrawalLimitInfo(USDTwithdrawalLimitInfoManage.SiteId,
                USDTwithdrawalLimitInfoManage.CurrencyId, USDTwithdrawalLimitInfoManage.VipLevel + 1, USDTwithdrawalLimitInfoManage.NetworkType);

            if (currentUpperPoint != null)
                return 0;

            return _withdrawalLimitService.DeleteUSDTWithdrawalLimitInfo(USDTwithdrawalLimitInfoManage, Operator);
        }

        public bool EditUSDTWithdrawalLimit(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage, string updater)
        {
            if (USDTwithdrawalLimitInfoManage.WithdrawalMax < 0 || USDTwithdrawalLimitInfoManage.WithdrawalMin < 0)
            {
                return false;
            }

            return _withdrawalLimitService.EditUSDTWithdrawalLimitInfo(USDTwithdrawalLimitInfoManage, updater);
        }

        public IEnumerable<CryptoWithdrawalLimitInfo> GetCryptoWithdrawalLimitInfos(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            int cryptoCurrencyId,
            string network,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {

            var result = _withdrawalLimitService.GetCryptoWithdrawalLimitInfos(
                JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIds),
                cryptoCurrencyId,
                network,
                 (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount).ToList();

            return result;
        }

        public IEnumerable<CryptoWithdrawalLimitInfo> GetCryptoWithdrawalLimitInfos(int siteId, int currencyId)
        {
            var result = _withdrawalLimitService.GetCryptoWithdrawalLimitInfos(siteId, currencyId);
            return result;
        }

        public bool AddCryptoWithdrawalLimit(ParamsWithLog<CreateCryptoWithdrawalLimitInfo> input)
        {
            if (input.After.WithdrawalMin < 0)
            {
                _logger.LogInformation($"Incorrect withdrawal limit min: {input.After.WithdrawalMin}");
                return false;
            }

            return _withdrawalLimitService.AddCryptoWithdrawalLimitInfo(input);
        }

        public bool DeleteCryptoWithdrawalLimit(ParamsWithLog<DeleteCryptoWithdrawalLimitInfo> input)
        {
            var currentUpperPointInput = new CryptoWithdrawalLimitInfoManage
            {
                NetworkType = input.Before.NetworkType,
                CryptoCurrency = input.Before.CryptoCurrency,
                VipLevel = input.Before.VipLevel + 1
            };

            var currentUpperPoint =
                _withdrawalLimitService.GetCryptoWithdrawalLimitInfo(currentUpperPointInput, input.SiteId, input.CurrencyId);

            if (currentUpperPoint != null)
            {
                _logger.LogInformation($"Current upper point is not null. Current VIP level: {input.Before.VipLevel}");
                return false;
            }

            return _withdrawalLimitService.DeleteCryptoWithdrawalLimitInfo(input);
        }

        public bool EditCryptoWithdrawalLimit(ParamsWithLog<UpdateCryptoWithdrawalLimitInfo> input)
        {
            if (input.After.WithdrawalMax < 0 || input.After.WithdrawalMin < 0)
            {
                _logger.LogInformation($"Incorrect withdrawal limit min: {input.After.WithdrawalMin}, max: {input.After.WithdrawalMax}");
                return false;
            }

            return _withdrawalLimitService.EditCryptoWithdrawalLimitInfo(input);
        }

        public bool EditCryptoWithdrawalLimit(ParamsWithLog<IEnumerable<UpdateCryptoWithdrawalLimitInfo>> input)
        {
            return _withdrawalLimitService.EditCryptoWithdrawalLimitInfo(input);
        }


        public CryptoInfoData GetCryptoInfoData(int siteId)
        {
            var infos = _paymentManageService.GetCryptoInfoList(siteId);
            var cryptoCurrency = infos
                .GroupBy(x => x.CurrencyId)
                .Select(x =>
                {
                    var d = x.First();
                    return new CryptoCurrency
                    {
                        CurrencyId = d.CurrencyId,
                        CurrencyName = d.CurrencyName
                    };
                });

            var cryptoNetwork = infos.Select(x => new CryptoNetwork
            {
                CurrencyId = x.CurrencyId,
                Network = x.Network,
                NetworkType = x.NetworkType
            });

            return new CryptoInfoData
            {
                CryptoCurrency = cryptoCurrency,
                CryptoNetwork = cryptoNetwork
            };
        }

        public decimal GetWithdrawalAvailableBalance(int custId)
            => _withdrawalService.GetWithdrawalAvailableBalance(custId);

        public async Task<(bool isSuccess, string TransId)> CreateWithdrawalAsync(int custId, ApplicationAccountTypeEnum paymentId, string cryptoNetwork, int cryptoCurrencyId,
            bool isExistBankCard, int sysId, string accountNo, string accountHolderName, decimal amount, decimal exchangeRate, decimal exchangeAmount, string memo, string updater)
        {
            var IFSCCode = string.Empty;
            var cryptoAccountNickName = string.Empty;

            CheckParameterForCreateWithdrawal(custId, paymentId, cryptoNetwork, cryptoCurrencyId, isExistBankCard, sysId, accountNo, accountHolderName, amount, exchangeRate, exchangeAmount, memo);

            var custInfo = _custInfoBLL.GetCustInfoWithCache(custId);

            CheckCustInfo(custInfo);

            if (isExistBankCard)
                CheckCustAccount(paymentId, cryptoCurrencyId, cryptoNetwork, custInfo, sysId, accountNo,
                out accountHolderName, out IFSCCode, out cryptoAccountNickName);

            CheckWithdrawalLimit(custInfo, paymentId, cryptoCurrencyId, cryptoNetwork, amount);

            CheckWithdrawalAmount(custId, custInfo.CurrencyId, amount);

            var convertData = ConvertData(paymentId, sysId, accountNo, accountHolderName, IFSCCode, cryptoAccountNickName, cryptoCurrencyId, cryptoNetwork, exchangeRate, exchangeAmount);

            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            if (!_withdrawalService.CreateWithdrawalInfo(custId, custInfo.UserName, custInfo.CurrencyId,
            custInfo.SiteId, amount, memo, updater, convertData.WithdrawalInfo, _common.GetEnv(), trans, out decimal afterAmount, out string transId))
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return (false, string.Empty);
            }

            var logResult = _adjustmentLogReportBLL.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = updater,
                SiteId = custInfo.SiteId,
                CurrencyId = custInfo.CurrencyId,
                ActionType = ActionType.Create,
                MenuEnum = AdminAdjustmentLogMenuEnum.WithdrawalSubmission,
                BeforeDetailInfo = null,
                AfterDetailInfo = new
                {
                    CustId = custInfo.CustId,
                    UserName = custInfo.UserName,
                    Payment = paymentId.ToString(),
                    UsedCard = isExistBankCard ? "Member Card" : "Manual Card",
                    AccountInfo = JsonConvert.SerializeObject(convertData.LogDetail),
                    Amount = amount,
                    Memo = memo,
                    Operator = updater
                }.ToLog()
            });

            if (!logResult)
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return (false, string.Empty);
            }
            await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
            return (true, transId);
        }

        #region WithdrawalBankAccounting

        //For Old Code
        //現行前端要求拆兩隻API
        public bool GetAccountingListByTransId(string transId, out GetWithdrawalInfoAndAccountingModel data)
        {
            data = new GetWithdrawalInfoAndAccountingModel();
            var withdrawalInfo = _withdrawalService.GetWithdrawalInfo(0, transId);

            if (withdrawalInfo == null)
            {
                return false;
            }
            else
            {
                if (withdrawalInfo.WithdrawalType == WithdrawalTypeEnum.LocalBank
                    && withdrawalInfo.Status == WithdrawalStatusEnum.Approved)
                {
                    var accountingList = GetAccountingList(transId, withdrawalInfo.SiteId, withdrawalInfo.CurrencyId);
                    var isoCode = _reportService.GetCurrencyList().Where(x => x.CurrencyId == withdrawalInfo.CurrencyId)
                        .Select(x => x.IsoCode).FirstOrDefault();
                    data.Withdrawal = withdrawalInfo;
                    data.AccountingList = accountingList;
                    data.IsoCode = isoCode;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool GetWithdrawalInfoByTransId(string transId, out WithdrawalRequest data)
        {
            data = _withdrawalService.GetWithdrawalInfo(0, transId);
            if (data == null)
            {
                return false;
            }

            return true;
        }


        public bool GetWithdrawalAccountingByTransId(string transId, out IEnumerable<GetWithdrawalBankAccountingModel> data)
        {
            data = null;
            var withdrawalInfo = _withdrawalService.GetWithdrawalInfo(0, transId);

            if (withdrawalInfo == null)
            {
                return false;
            }
            else
            {
                if (withdrawalInfo.WithdrawalType == WithdrawalTypeEnum.LocalBank
                    && withdrawalInfo.Status == WithdrawalStatusEnum.Approved)
                {
                    data = GetAccountingList(transId, withdrawalInfo.SiteId, withdrawalInfo.CurrencyId);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerable<WithdrawalAccountingModel> GetWithdrawalAccountingByTransId(string transId)
        {
            var result = _withdrawalBankAccountingService.GetByTransId(transId);
            return result;
        }

        #endregion

        public bool EditEWalletWithdrawalLimit(ParamsWithLog<EWalletUpdateRequest> input)
        {
            if (input.After.WithdrawalMax < 0
                || input.After.WithdrawalMin < 0)
            {
                return false;
            }

            return _withdrawalLimitService.EditEWalletWithdrawalLimitInfo(input);
        }

        public bool UpdateEWalletWithdrawalLimitInfoStatus(ParamsWithLog<EWalletSwitchStatusRequest> input)
        {
            return _withdrawalLimitService.UpdateEWalletWithdrawalLimitInfoStatus(input);
        }

        public IEnumerable<EWalletWithdrawalLimitInfoManage> GetEWalletWithdrawalLimitInfo(int siteId)
        {
            return _withdrawalLimitService.GetEWalletWithdrawalLimitInfo(siteId);
        }

        public async Task<ApiResult<WithdrawalManualResponse>> WithdrawalManualApproval(WithdrawalManualRequest withdrawalManualRequest)
        {
            var url = $"{_payApiUrl}WithdrawalManualApprove";
            var response = await _courierAdapter.MyPostAsync<WithdrawalManualResponse, WithdrawalManualRequest>(url, withdrawalManualRequest);
            return response;
        }

        public async Task<ApiResult<WithdrawalManualResponse>> WithdrawalManualReject(WithdrawalManualRequest withdrawalManualRequest)
        {
            var url = $"{_payApiUrl}WithdrawalManualReject";
            var response = await _courierAdapter.MyPostAsync<WithdrawalManualResponse, WithdrawalManualRequest>(url, withdrawalManualRequest);
            return response;
        }

        public int SetWithdrawalMessage(ParamsWithLog<WithdrawalMessageRequest> input)
        {
            var result = 1;

            if (!_withdrawalService.SetWithdrawalMessage(input))
            {
                return 0;
            }

            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = input.Operator,
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                RefTransId = input.TransId,
                ActionType = ActionType.Create,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetWithdrawalMessage,
                BeforeDetailInfo = null,
                AfterDetailInfo = JsonConvert.SerializeObject(input.After)
            };

            _adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam);
            _withdrawalService.SetWithdrawalLog(input.After.WithdrawalId, input.SiteId, $"SendMessage:{input.After.StatusName}", (int)input.After.Action, input.Operator);

            return result;
        }

        public async Task<bool> SetWithdrawalProcessWay(string transId, WithdrawalInfoProcessWayEnum processWay)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var withdrawalId);
            return await _withdrawalService.SetWithdrawalProcessWay(withdrawalId, processWay);
        }


        public IEnumerable<WithdrawalMessage> GetWithdrawalMessageByWithdrawalIds(IEnumerable<int> withdrawalIds)
        {
            return _withdrawalService.GetWithdrawalMessage(withdrawalIds);
        }

        #endregion

        #region Private Methods


        private string GetTimeString(TimeSpan ts)
        {
            var tempDays = "";
            var tempHours = "";
            var tempMinutes = "";
            var tempSeconds = "";
            var result = "";
            if (ts.Days != 0)
            {
                tempDays = string.Format("{0}Days", ts.Days.ToString());
            }

            if (ts.Hours >= 10)
            {
                tempHours = string.Format("{0}", ts.Hours.ToString());
            }
            else
            {
                tempHours = string.Format("0{0}", ts.Hours.ToString());
            }

            if (ts.Minutes >= 10)
            {
                tempMinutes = string.Format("{0}", ts.Minutes.ToString());
            }
            else
            {
                tempMinutes = string.Format("0{0}", ts.Minutes.ToString());
            }

            if (ts.Seconds >= 10)
            {
                tempSeconds = string.Format("{0}", ts.Seconds.ToString());
            }
            else
            {
                tempSeconds = string.Format("0{0}", ts.Seconds.ToString());
            }

            result = string.Format("{0}{1}:{2}:{3}", tempDays, tempHours, tempMinutes, tempSeconds);
            return result;
        }

        private IEnumerable<GetWithdrawalBankAccountingModel> GetAccountingList(string transId, int siteId, int currencyId)
        {
            return _withdrawalBankAccountingService.GetAccountingListByTransId(transId, siteId, currencyId);
        }

        #region Withdrawal Submission

        private void CheckParameterForCreateWithdrawal(int csutId, ApplicationAccountTypeEnum paymentId,
            string cryptoNetwork, int cryptoCurrencyId, bool isExistBankCard,
            int sysId, string accountNo, string accountHolderName,
            decimal amount, decimal exchangeRate, decimal exchangeAmount, string memo)
        {
            if (csutId == 0)
                throw new AdminException(ErrorCodeEnum.Fail, "CustId is invalid.");
            if (paymentId == 0)
                throw new AdminException(ErrorCodeEnum.Fail, "Payment is invalid.");
            if (paymentId == ApplicationAccountTypeEnum.Crypto && (string.IsNullOrWhiteSpace(cryptoNetwork) || cryptoCurrencyId == 0))
                throw new AdminException(ErrorCodeEnum.Fail, "Crypto Channel is invalid.");
            if (paymentId == ApplicationAccountTypeEnum.Crypto && (exchangeRate == 0 || exchangeAmount == 0))
                throw new AdminException(ErrorCodeEnum.Fail, "Missing crypto exchange info.");
            if (paymentId != ApplicationAccountTypeEnum.Crypto && sysId == 0)
                throw new AdminException(ErrorCodeEnum.Fail, "SysId is invalid.");
            if (string.IsNullOrWhiteSpace(accountNo))
                throw new AdminException(ErrorCodeEnum.Fail, "AccountNo is invalid.");
            if (!isExistBankCard && string.IsNullOrWhiteSpace(accountHolderName))
                throw new AdminException(ErrorCodeEnum.Fail, "AccountHolderName is invalid.");
            if (amount <= 0)
                throw new AdminException(ErrorCodeEnum.Fail, "Amount is Invaild.");
            if (string.IsNullOrWhiteSpace(memo))
                throw new AdminException(ErrorCodeEnum.Fail, "Memo is Invaild.");
        }

        private void CheckCustInfo(Customer custInfo)
        {
            if (custInfo.CurrencyId == 15 && !custInfo.PhoneVerified)
                throw new AdminException(ErrorCodeEnum.Fail, "PhoneVerified is Invaild.");

            var isExist = _withdrawalService.IsExistRunningWithdrawalInfo(custInfo.CustId);
            if (isExist)
                throw new AdminException(ErrorCodeEnum.Fail, "Exist running withdrawal. Please check.");

            if (custInfo.ActStatus == (int)ActStatusEnum.NonDepoist)
                throw new AdminException(ErrorCodeEnum.Fail, "Haven't deposit yet.");
        }

        private void CheckCustAccount(ApplicationAccountTypeEnum paymentId, int cryptoCurrencyId, string network,
            Customer custInfo, int sysId, string accountNo, out string accountName, out string IFSCCode, out string cryptoAccountNickName)
        {
            accountName = string.Empty;
            IFSCCode = string.Empty;
            cryptoAccountNickName = string.Empty;

            switch (paymentId)
            {
                //check withdrawal count and daily and withdrawal limit
                case ApplicationAccountTypeEnum.LocalBank:
                    {
                        var custBankList = _custInfoBLL.GetCustBankDetailAsync(custInfo.CustId).Result;

                        if (custBankList == null
                            || !custBankList.Any()
                            || !custBankList.Any(x => x.SysId == sysId && x.AccountNo == accountNo))
                            throw new AdminException(ErrorCodeEnum.Fail, "This LocalBank AccountNo is not exist. Please Check.");
                        else
                        {
                            var accountInfo = custBankList.First(x => x.SysId == sysId && x.AccountNo == accountNo);
                            accountName = custInfo.AccountName;
                            IFSCCode = accountInfo.IFSCCode;
                        }
                        break;
                    }
                case ApplicationAccountTypeEnum.Crypto:
                    {
                        var custCryptoInfoList = _custInfoBLL.GetCustCryptoInfoListAsync(custInfo.CustId).Result;
                        if (custCryptoInfoList == null
                            || !custCryptoInfoList.Any()
                            || !custCryptoInfoList.Any(x => x.USDTAddress == accountNo && x.CurrencyId == cryptoCurrencyId && x.Network == network))
                            throw new AdminException(ErrorCodeEnum.Fail, "This Crypto AccountNo is not exist. Please Check.");
                        else
                        {
                            var accountInfo = custCryptoInfoList.First(x => x.USDTAddress == accountNo && x.CurrencyId == cryptoCurrencyId && x.Network == network);
                            accountName = custInfo.AccountName;
                            cryptoAccountNickName = accountInfo.NickName;
                        }
                        break;
                    }
                //case ApplicationAccountTypeEnum.EWallet:
                //    {
                //        var eWalletInfo = _custInfoBLL.GetCustEWalletListAsync(custInfo.CustId).Result;
                //        if (custCryptoInfoList == null
                //            || !custCryptoInfoList.Any()
                //            || eWalletInfo.Any(x => x.AccountNo == accountNo && x.SysId == sysId))
                //        {
                //            return apiResult;
                //        }
                //        break;
                //    }
                default:
                    {
                        break;
                    }
            }
        }

        private void CheckWithdrawalLimit(Customer custInfo, ApplicationAccountTypeEnum paymentId, int cryptoCurrencyId, string network, decimal amount)
        {
            var custDailySum = _withdrawalService.GetDailyWithdrawalSummary(custInfo.CustId);
            switch (paymentId)
            {
                //check withdrawal count and daily and withdrawal limit
                case ApplicationAccountTypeEnum.LocalBank:
                    {
                        var withdrawalLimitInfo = GetWithdrawalLimitInfos(new List<int> { custInfo.SiteId }, new List<int> { custInfo.CurrencyId }, 1, 100, out _, out _).First(x => x.VipLevel == custInfo.UserLevel);

                        if (withdrawalLimitInfo != null)
                            IsValidWithWithdrawalLimit(amount, custDailySum.Amount, custDailySum.Count,
                                withdrawalLimitInfo.WithdrawalMin, withdrawalLimitInfo.TotalWithdrawalMax, withdrawalLimitInfo.Times);
                        break;
                    }
                case ApplicationAccountTypeEnum.Crypto:
                    {
                        var withdrawalCryptoLimitInfo = GetCryptoWithdrawalLimitInfos(new List<int> { custInfo.SiteId }, new List<int> { custInfo.CurrencyId }, cryptoCurrencyId, network, 1, 100, out _, out _).First(x => x.VipLevel == custInfo.UserLevel);

                        if (withdrawalCryptoLimitInfo != null)
                            IsValidWithWithdrawalLimit(amount, custDailySum.Amount, custDailySum.Count,
                                withdrawalCryptoLimitInfo.WithdrawalMin, withdrawalCryptoLimitInfo.WithdrawalMax, withdrawalCryptoLimitInfo.Times);

                        break;
                    }
                //case ApplicationAccountTypeEnum.EWallet:
                //    {
                //        break;
                //    }
                default:
                    {
                        break;
                    }
            }
        }

        private void CheckWithdrawalAmount(int custId, int currencyId, decimal amount)
        {
            var checkedAmount = amount;
            var balance = _custInfoBLL.GetBalance(custId);
            var WithdrawalAvailableBalance = GetWithdrawalAvailableBalance(custId);

            if (WithdrawalAvailableBalance < amount)
                throw new AdminException(ErrorCodeEnum.Fail, $"Withdrawal Available Balance={WithdrawalAvailableBalance} < Amount={amount}. Please Check.");

            if (balance < amount)
                throw new AdminException(ErrorCodeEnum.Fail, $"Balance={balance} < Amount={amount}. Please Check.");

            if (_isConvert.ContainsKey(currencyId))
                checkedAmount /= 1000;

            if (Math.Ceiling(checkedAmount) != Math.Floor(checkedAmount))
                throw new AdminException(ErrorCodeEnum.Fail, $"Check Amount Fail. Please Check.");
        }


        private void IsValidWithWithdrawalLimit(decimal amount, decimal dailyAmount, int dailyCount,
            decimal withdrawalMin, decimal withdrawalMax, int withdrawalTimes)
        {
            if (withdrawalMin > amount)
                throw new AdminException(ErrorCodeEnum.Fail, $"Amount={amount} < WithdrawalMin={withdrawalMin}. Please Check.");


            if (withdrawalMax != 0 && (dailyAmount + amount) > withdrawalMax)
                throw new AdminException(ErrorCodeEnum.Fail, $"(DailyAmount + Amount)={dailyAmount + amount} > WithdrawalMax={withdrawalMax}. Please Check.");


            if (withdrawalTimes != 0 && dailyCount >= withdrawalTimes)
                throw new AdminException(ErrorCodeEnum.Fail, $"Daily Withdrawal={dailyCount} >= WithdrawalDailyTime={withdrawalTimes}. Please Check.");

        }

        private (string WithdrawalInfo, object LogDetail) ConvertData(ApplicationAccountTypeEnum paymentId,
              int sysId, string accountNo, string accountName, string IFSCCode,
            string accountNickName, int cryptoCurrencyId, string network, decimal exchangeRate, decimal exchangeAmount)
        {
            Object data = new { };
            Object logDetail = new { };

            switch (paymentId)
            {
                case ApplicationAccountTypeEnum.LocalBank:
                    {
                        var bankCode = _bankDetailBll.GetSysBankList().First(x => x.SysId == sysId).BankCode;
                        data = new
                        {
                            SysId = sysId,
                            AccountName = accountName,
                            AccountNo = accountNo,
                            IFSCCode = IFSCCode
                        };
                        logDetail = new
                        {
                            BankName = $"{sysId}-{bankCode}",
                            AccountNo = accountNo,
                            AccountName = accountName,
                        };
                        break;
                    }
                case ApplicationAccountTypeEnum.Crypto:
                    {
                        var cryptoCurrency = _reportService.GetCurrencyList().First(x => x.CurrencyId == cryptoCurrencyId).IsoCode;
                        data = new
                        {
                            AccountType = "crypto",
                            CurrencyId = cryptoCurrencyId,
                            Network = network,
                            AccountNo = accountNo,
                            AccountName = accountName,
                            NickName = string.IsNullOrWhiteSpace(accountNickName) ? accountName : accountNickName,
                            TempExchangeRate = exchangeRate,
                            TempExchangeAmount = exchangeAmount
                        };
                        logDetail = new
                        {
                            Channel = $"{cryptoCurrency}-{network}",
                            AccountNo = accountNo,
                            AccountName = accountName,
                            NickName = string.IsNullOrWhiteSpace(accountNickName) ? accountName : accountNickName,
                            TempExchangeRate = exchangeRate,
                            TempExchangeAmount = exchangeAmount
                        };
                        break;
                    }
                //case ApplicationAccountTypeEnum.EWallet:
                //    {
                //        var bankCode = _bankDetailBll.GetSysBankList().First(x => x.SysId == sysId).BankCode;
                //        data = new
                //        {
                //            AccountType = "ewallet",
                //            EWalletSysId = sysId,
                //            AccountNo = accountNo,
                //            AccountName = accountName
                //        };
                //        logDetail = new
                //        {
                //            SysId = sysId,
                //            SysName = bankCode,
                //            AccountNo = accountNo,
                //            accountName = accountName,
                //        };
                //        break;
                //    }
                default:
                    {
                        break;
                    }
            }
            return (JsonConvert.SerializeObject(data), logDetail);
        }

        #endregion

        #endregion
    }
}