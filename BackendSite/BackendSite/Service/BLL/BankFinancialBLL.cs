using System.Transactions;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using NuGet.Packaging;
using static BackendSite.Controllers.ApiV2.Finance.Adjustment.BalanceAdjustmentController;

namespace BackendSite.Service.BLL
{
    public class BankFinancialBLL
    {
        #region Fields

        private readonly BankFinancialService _bankFinancialService;
        private readonly PaymentManageService _paymentManageService;
        private readonly ReportService _reportService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly AWSS3Helper _awsS3Helper;
        private readonly ICacheHelper _cacheHelper;
        private readonly string _bucketName;
        private readonly string _bucketUrl;
        private readonly string _folder;
        private readonly Cryptor _cryptor;
        private readonly ServerInfoBLL _serverInfoBll;
        
        #endregion

        #region Constructors

        public BankFinancialBLL(IServiceProvider provider)
        {
            _bankFinancialService = provider.GetRequiredService<BankFinancialService>();
            _paymentManageService = provider.GetRequiredService<PaymentManageService>();
            _reportService = provider.GetRequiredService<ReportService>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _bucketName = configBll.GetConfigValue("AWSBackOfficeBucketName");
            _bucketUrl = configBll.GetConfigValue("AWSBackOfficeBucketUrl");
            _folder = configBll.GetConfigValue("AWSFinanceFolderName");
            _cryptor = provider.GetRequiredService<Cryptor>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region Library Data

        public IEnumerable<BankFinancialReasonModel> GetReason(string funcKey)
        {
            var key = $"Reason:{funcKey}";

            return _cacheHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(5),
                () => _bankFinancialService.GetReason(funcKey));
        }

        public IEnumerable<BankFinancialReasonModel> GetReasonByAdjustmentType(AdjustmentType adjustmentType)
        {
            return  _bankFinancialService.GetReasonByAdjustmentType(adjustmentType);
        }

        [Obsolete("請改用GetPayList(int siteId)")]
        public IEnumerable<PayList> GetPayList()
        {
            return _paymentManageService.GetPayList();
        }

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountList()
        {
            return _bankFinancialService.GetTransferAccountList();
        }


        public IEnumerable<BankFinancialTransferAccount> GetTransferAccountList(int siteId)
        {
            return _bankFinancialService.GetTransferAccountList(siteId, new[]
            {
                TransferAccountPurposeEnum.Transfer
            });
        }

        public IEnumerable<BankFinancialTransferAccount> GetFinancialAccountList(int siteId, IEnumerable<TransferAccountPurposeEnum> purposes)
        {
            return _bankFinancialService.GetTransferAccountList(siteId, purposes);
        }

        #endregion

        #region Transfer Account Management

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoList(GetTransferAccountInfoListParams input)
        {
            return _bankFinancialService.GetTransferAccountInfoListV2(input);
        }

        public IEnumerable<string> GetTransferAccountAgentList(int siteId)
        {
            return _bankFinancialService.GetTransferAccountAgentList(siteId)
                .Where(x => !string.IsNullOrEmpty(x));
        }

        public (bool Status, int Count, string Message) SetTransferAccount(BankFinancialTransferAccountModel input, string updater)
        {
            var count = 0;
            var updateTransferDataFlag = false;
            var affectTransferData = 0;
            var oldData = new BankFinancialTransferAccountDetailModel();
            var isCreate = input.Id == 0;
            var siteIds = _serverInfoBll.GetSiteList().Select(x => x.SiteId);

            if (!CheckParameterForSetTransferAccount(input, siteIds, out var message))
            {
                return (false, count, message);
            }

            if (!isCreate)
            {
                oldData = GetTransferAccountInfoList(new GetTransferAccountInfoListParams(input.Id))
                    .FirstOrDefault();
            }

            var transferAccountModel = new BankFinancialTransferAccountModel
            {
                Id = input.Id,
                SiteId = input.SiteId,
                SiteName = input.SiteName,
                CurrencyId = input.CurrencyId,
                SysId = input.SysId,
                Purpose = input.Purpose,
                Channel = input.Channel,
                Agent = input.Agent == "" ? null : input.Agent,
                PayId = input.PayId == 0 ? null : input.PayId,
                CryptoCurrencyId = input.CryptoCurrencyId,
                Network = input.Network,
                AccountCode = string.IsNullOrWhiteSpace(input.AccountCode) ? string.Empty : input.AccountCode,
                AccountNo = string.IsNullOrWhiteSpace(input.AccountNo) ? string.Empty : input.AccountNo,
                AccountName = input.AccountName,
                DisplayName = input.DisplayName,
                FrontendDisplayName = input.FrontendDisplayName ?? string.Empty,
                BankLoginUrl = string.IsNullOrWhiteSpace(input.BankLoginUrl) ? string.Empty : input.BankLoginUrl,
                BankLoginAccount = string.IsNullOrWhiteSpace(input.BankLoginAccount) ? string.Empty : _cryptor.Encrypt(input.BankLoginAccount),
                BankLoginPassword = string.IsNullOrWhiteSpace(input.BankLoginPassword) ? string.Empty : _cryptor.Encrypt(input.BankLoginPassword),
                LoginPin = string.IsNullOrWhiteSpace(input.LoginPin) ? string.Empty : _cryptor.Encrypt(input.LoginPin),
                MobilePin = string.IsNullOrWhiteSpace(input.MobilePin) ? string.Empty : _cryptor.Encrypt(input.MobilePin),
                TransferPin = string.IsNullOrWhiteSpace(input.TransferPin) ? string.Empty : _cryptor.Encrypt(input.TransferPin),
                CorporateID = string.IsNullOrWhiteSpace(input.CorporateID) ? string.Empty : _cryptor.Encrypt(input.CorporateID),
                InitialAmount = input.InitialAmount,
                InitialTime = input.InitialTime,
                MaxBalanceAlertAmount = input.MaxBalanceAlertAmount,
                Reason = input.Reason == 0 ? null : input.Reason,
                Remark = input.Remark,
                Status = input.Status,
                Creator = isCreate ? input.Updator : oldData.Creator,
                Updator = input.Updator
            };

            var logParams = new CreateAdminAdjustmentInfoLogInput
            {
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                Operator = updater,
                ActionType = isCreate ? ActionType.Create : ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetFinanceAccount,
                BeforeDetailInfo = isCreate ? null : JsonConvert.SerializeObject(new
                {
                    Id = oldData.Id,
                    Purpose = GetPurpose(oldData.Purposes.Select(p => (int)p).OrderBy(p => p)),
                    Channel = oldData.Channel.ToString(),
                    Agent = oldData.Agent,
                    AccountNo = oldData.AccountNo,
                    AccountName = oldData.AccountName,
                    DisplayName = oldData.DisplayName,
                    oldData.FrontendDisplayName,
                    oldData.BankLoginUrl,
                    BankLoginAccount = oldData.BankLoginAccount,
                    BankLoginPassword = oldData.BankLoginPassword,
                    TransferPin = oldData.TransferPin,
                    MobilePin = oldData.MobilePin,
                    LoginPin = oldData.LoginPin,
                    CorporateID = oldData.CorporateID,
                    InitialAmount = oldData.InitialAmount,
                    InitialTime = oldData.InitialTime.ToString("yyyy-MM-ddTHH:mm:ss+00:00"),
                    MaxBalanceAlertAmount = oldData.MaxBalanceAlertAmount,
                    Remark = oldData.Remark,
                    Status = oldData.Status.ToString()
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    Purpose = GetPurpose(input.Purpose),
                    Channel = input.Channel.ToString(),
                    Agent = input.Agent,
                    AccountNo = input.AccountNo,
                    AccountName = input.AccountName,
                    DisplayName = input.DisplayName,
                    input.FrontendDisplayName,
                    input.BankLoginUrl,
                    transferAccountModel.BankLoginAccount,
                    transferAccountModel.BankLoginPassword,
                    transferAccountModel.TransferPin,
                    transferAccountModel.MobilePin,
                    transferAccountModel.LoginPin,
                    transferAccountModel.CorporateID,
                    InitialAmount = input.InitialAmount,
                    InitialTime = input.InitialTime.ToString("yyyy-MM-ddTHH:mm:ss+00:00"),
                    MaxBalanceAlertAmount = input.MaxBalanceAlertAmount,
                    Remark = input.Remark,
                    Status = input.Status.ToString()
                })
            };

            if (isCreate)
            {
                if (input.Channel == TransferChannelEnum.LocalBank
                    && _bankFinancialService.CheckTransferAccountExists(input.SiteId, transferAccountModel.AccountNo))
                {
                    return (false, count, $"AccountNo: {transferAccountModel.AccountNo} already exists.");
                }

                using (var scope = new TransactionScope())
                {
                    if (_bankFinancialService.InsertTransferAccount(transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (false, count, "Insert Transfer Account Fail. Duplicate AccountNo.");
                    }

                    if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParams))
                    {
                        scope.Dispose();

                        return (false, count, "Insert Activity Log Fail.");
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Insert Transfer Account And Insert Log Success.";
            }
            else
            {
                using (var scope = new TransactionScope())
                {
                    if (_bankFinancialService.UpdateTransferAccount(transferAccountModel) != 1)
                    {
                        scope.Dispose();

                        return (false, count, "Update Transfer Account Fail.");
                    }

                    if (updateTransferDataFlag)
                    {
                        if (_bankFinancialService.UpdateTransferInfo(input, oldData) != affectTransferData)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Data Fail.");
                        }
                    }

                    if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParams))
                    {
                        scope.Dispose();

                        return (false, count, "Insert Activity Log Fail.");
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Update Transfer Account And Insert Log Success.";
            }
            
            _ = Task.Run(() => _cacheHelper.KeyDeleteByPattern("PaySetting:*", RedisDataBase.Member));

            return (true, count, message);
        }

        #region Private Methods For GetTransferAccountInfoList

        private string GetTransferId()
        {
            const string prefix = "T";
            var (nowDate, nowMaxNo) = _bankFinancialService.NoAssign(prefix);

            return prefix + nowDate + nowMaxNo.PadLeft(4, '0');
        }

        #endregion

        #region Private Methods Set TransferAccount

        private bool CheckParameterForSetTransferAccount(BankFinancialTransferAccountModel input,
            IEnumerable<int> SiteIds,
            out string message)
        {
            message = string.Empty;

            if (input == null)
            {
                message = "Data Invalid.";

                return false;
            }

            #region Parameter Values Check

            if (input.Id == null)
            {
                message = "Data-[Id] invalid.Notify admin please.";
                return false;
            }

            if (input.SiteId == null)
            {
                message = "Data-[Site] invalid.";
                return false;
            }

            if (!SiteIds.Contains(input.SiteId))
            {
                message = "Data-[Site] not match.";

                return false;
            }

            if (input.SiteName == null)
            {
                message = "Data-[SiteName] Invalid.";

                return false;
            }

            if (input.CurrencyId == null || input.CurrencyId == 0)
            {
                message = "Data-[Currency] invalid.";

                return false;
            }

            if (input.Channel == TransferChannelEnum.LocalBank && (input.SysId == null || input.SysId == 0))
            {
                message = "Data-[Bank Code] invalid.";

                return false;
            }

            if (input.Channel == null || input.Channel == 0)
            {
                message = "Data-[Channel] invalid.";

                return false;
            }

            if (input.Channel == TransferChannelEnum.PSP &&
                (input.PayId == null || input.PayId == 0))
            {
                message = "Data-[Channel] is PSP.But [Pay] is invalid.";

                return false;
            }

            if ((input.Channel == TransferChannelEnum.LocalBank || input.Channel == TransferChannelEnum.HeadOffice) &&
               input.Agent == null)
            {
                message = "Data-[Channel] is Local Bank or Head Office.But [Agent] is invalid.";

                return false;
            }

            if (input.AccountNo == null)
            {
                message = "Data-[AccountNo] invalid.";

                return false;
            }

            if (input.AccountName == null)
            {
                message = "Data-[AccountName] invalid.";

                return false;
            }

            if (input.DisplayName == null)
            {
                message = "Data-[DisplayName] invalid.";

                return false;
            }

            if (input.InitialAmount == null || input.InitialAmount < 0)
            {
                message = "Data-[InitialAmount] invalid.";

                return false;
            }

            if (input.MaxBalanceAlertAmount == null || input.MaxBalanceAlertAmount < 0)
            {
                message = "Data-[MaxBalanceAlertAmount] invalid.";

                return false;
            }

            if (input.Status == null)
            {
                message = "Data-[Status] invalid.";

                return false;
            }

            if (input.CreateTime == null)
            {
                message = "Data-CreateTime invalid.Notify admin please.";

                return false;
            }

            if (input.Creator == null)
            {
                message = "Data-Creator invalid.Notify admin please.";

                return false;
            }

            if (input.UpdateTime == null)
            {
                message = "Data-UpdateTime invalid.Notify admin please.";

                return false;
            }

            if (input.Updator == null)
            {
                message = "Data-Updator invalid.Notify admin please.";

                return false;
            }

            #endregion


            return true;
        }

        private string GetPurpose(IEnumerable<int> purpose)
        {
            var result = string.Empty;
            foreach (var i in purpose)
            {
                switch (i)
                {
                    case 1: result += ",Deposit"; break;
                    case 2: result += ",Withdrawal"; break;
                    case 3: result += ",Transfer"; break;
                }

            }
            return result.Substring(1);
        }

        #endregion

        #endregion

        #region Transfer Info Management

        public IEnumerable<BankFinancialTransferInfoTransDetailModel> GetFinancialTransferInfoList(
            GetTransferInfoListParams input, int pageNumber, int pageSize, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var (startIndex, _) = pageNumber.GetStartIndexAndPageSize(pageSize);
            var result =
                _bankFinancialService.GetFinancialTransferInfoList(input, startIndex, pageSize, out _, out var rowCount);

            pageInfo.SetPageInfo(rowCount, pageNumber, pageSize);
            return result;
        }

        public IEnumerable<BankFinancialTransferInfoTransDetailModel> ExportFinancialTransferInfoList(GetTransferInfoListParams input)
        {
            input.IsExport = true;
            var result =
                _bankFinancialService.GetFinancialTransferInfoList(input, 0, 0, out _, out _);

            return result;
        }

        public BankFinancialTransferInfoTransDetailModel GetFinancialTransferInfoById(string transferId)
        {
            var result =
                _bankFinancialService.GetFinancialTransferInfoById(transferId);

            return result.FirstOrDefault();
        }

        public async Task<(bool Status, int Count, string Message)> SetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail, IFormFile transferReceipt)
        {
            var count = 0;

            #region Checnk Parameter

            if (!CheckSharedParameterForSetTransferInfo(bankFinancialTransferInfoDetail, out var message))
            {
                return (false, count, message);
            }

            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus)
            {
                if (!CheckParameterForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
                {
                    return (false, count, message);
                }
            }
            else
            {
                if (!CheckParameterWithTypeOtherForSetTransferInfo(bankFinancialTransferInfoDetail, out message))
                {
                    return (false, count, message);
                }
            }

            #endregion

            #region SetParameterModel

            var (transferInfoFromModel, transferInfoToModel) = SetParameterModelForSetTransferInfo(bankFinancialTransferInfoDetail);

            #endregion

            if (bankFinancialTransferInfoDetail.TransferId == null)
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var transferId = GetTransferId();
                    transferInfoFromModel.TransferId = transferId;
                    transferInfoToModel.TransferId = transferId;

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {
                        if (_bankFinancialService.InsertTransferInfo(transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Minus Data Fail.");
                        }
                    }

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.InsertTransferInfo(transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Info Plus Data Fail.");
                        }
                    }

                    if (transferReceipt != null)
                    {
                        if (!await InsertTransferReceipt(transferReceipt, transferId))
                        {
                            scope.Dispose();

                            return (false, count, "Insert Transfer Receipt Fail.");
                        }
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Insert Transfer Info And Insert Log Success.";
            }
            else
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var before = GetFinancialTransferInfoById(bankFinancialTransferInfoDetail.TransferId);
                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
                    {

                        if (_bankFinancialService.UpdateTransferInfo(transferInfoFromModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Info Minus Data Fail.");
                        }
                    }

                    if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus ||
                       bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
                    {
                        if (_bankFinancialService.UpdateTransferInfo(transferInfoToModel) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Info Plus Data Fail.");
                        }
                    }

                    if (transferReceipt != null)
                    {
                        if (!await InsertTransferReceipt(transferReceipt, bankFinancialTransferInfoDetail.TransferId))
                        {
                            scope.Dispose();

                            return (false, count, "Update Transfer Receipt Fail.");
                        }
                    }

                    var after = GetFinancialTransferInfoById(bankFinancialTransferInfoDetail.TransferId);

                    #region logParams

                    var logParams = new CreateAdminAdjustmentInfoLogInput
                    {
                        SiteId = bankFinancialTransferInfoDetail.FromSiteId,
                        CurrencyId = bankFinancialTransferInfoDetail.FromCurrencyId,
                        Operator = bankFinancialTransferInfoDetail.UpdatorName,
                        ActionType = ActionType.Update,
                        MenuEnum = AdminAdjustmentLogMenuEnum.TransferInfoManagement,
                        BeforeDetailInfo = JsonConvert.SerializeObject(new
                        {
                            before.TransferId,
                            TransactionType = before.TransactionType.ToString(),
                            TransferType = before.TransferType.ToString(),
                            before.TransDate,
                            FromSite = before.FromSiteName,
                            FromCurrency = before.FromIsoCode,
                            FromChannel = before.FromChannel.ToString(),
                            before.FromAgent,
                            before.FromPayName,
                            before.FromUserName,
                            before.FromIsoCode,
                            before.FromBankCode,
                            before.FromAccountNo,
                            before.FromAccountDisplayName,
                            before.FromAmount,
                            before.FromFee,
                            ToSite = before.ToSiteName,
                            ToCurrency = before.ToIsoCode,
                            ToChannel = before.ToChannel.ToString(),
                            before.ToAgent,
                            before.ToPayName,
                            before.ToUserName,
                            before.ToIsoCode,
                            before.ToBankCode,
                            before.ToAccountNo,
                            before.ToAccountDisplayName,
                            before.ToAmount,
                            before.ToFee,
                            before.Remark
                        }),
                        AfterDetailInfo = JsonConvert.SerializeObject(new
                        {
                            TransactionType = after.TransactionType.ToString(),
                            TransferType = after.TransferType.ToString(),
                            after.TransDate,
                            FromSite = after.FromSiteName,
                            FromCurrency = after.FromIsoCode,
                            FromChannel = after.FromChannel.ToString(),
                            after.FromAgent,
                            after.FromPayName,
                            after.FromUserName,
                            after.FromIsoCode,
                            after.FromBankCode,
                            after.FromAccountNo,
                            after.FromAccountDisplayName,
                            after.FromAmount,
                            after.FromFee,
                            ToSite = after.ToSiteName,
                            ToCurrency = after.ToIsoCode,
                            ToChannel = after.ToChannel.ToString(),
                            after.ToAgent,
                            after.ToPayName,
                            after.ToUserName,
                            after.ToIsoCode,
                            after.ToBankCode,
                            after.ToAccountNo,
                            after.ToAccountDisplayName,
                            after.ToAmount,
                            after.ToFee,
                            after.Remark
                        })
                    };

                    #endregion

                    if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParams))
                    {
                        scope.Dispose();

                        return (false, count, "Insert Transfer Info, Create Admin Adjustment Info Log Fail.");
                    }

                    scope.Complete();
                }

                count = 1;
                message = "Update Transfer Info And Insert Log Success.";
            }

            return (true, count, message);
        }

        public (bool Status, int Count, string Message) DeleteTransferInfo(string transferId, string @operator = "")
        {
            var count = 0;
            string message;

            if (transferId != null)
            {
                //Step1:Get Data
                var data = GetFinancialTransferInfoById(transferId);
                var transferInfoReceiptModel = _bankFinancialService.GetTransferInfoReceipt(transferId);

                if (data == null)
                {
                    return (false, count, "Delete Transfer Info Data Fail.Data is not exist.");
                }

                //Step2: Set Data for Insert Log
                var (transferInfoFromModel, transferInfoToModel) = SetParameterModelForSetTransferInfo(data);

                using (var scope = new TransactionScope())
                {
                    var affectCount = 0;

                    //Step4:Delete Data
                    if (_bankFinancialService.DeleteTransferInfo(transferId) < 1)
                    {
                        scope.Dispose();

                        return (false, count, "Delete Transfer Info Data Fail.");
                    }

                    if (transferInfoReceiptModel != null)
                    {
                        if (_bankFinancialService.DeleteTransferInfoReceipt(transferId) != 1)
                        {
                            scope.Dispose();

                            return (false, count, "Delete Transfer Info Receipt Fail.");
                        }

                        _ = _awsS3Helper.DeleteFileAsync(_bucketName, transferInfoReceiptModel.PicFileName);
                    }

                    var logParams = new CreateAdminAdjustmentInfoLogInput
                    {
                        SiteId = data.FromSiteId,
                        CurrencyId = data.FromCurrencyId,
                        Operator = @operator,
                        ActionType = ActionType.Delete,
                        MenuEnum = AdminAdjustmentLogMenuEnum.TransferInfoManagement,
                        BeforeDetailInfo = JsonConvert.SerializeObject(new
                        {
                            data.TransferId,
                            TransactionType = data.TransactionType.ToString(),
                            TransferType = data.TransferType.ToString(),
                            data.TransDate,
                            FromSite = data.FromSiteName,
                            FromCurrency = data.FromIsoCode,
                            FromChannel = data.FromChannel.ToString(),
                            data.FromAgent,
                            data.FromPayName,
                            data.FromUserName,
                            data.FromIsoCode,
                            data.FromBankCode,
                            data.FromAccountNo,
                            data.FromAccountDisplayName,
                            data.FromAmount,
                            data.FromFee,
                            ToSite = data.ToSiteName,
                            ToCurrency = data.ToIsoCode,
                            ToChannel = data.ToChannel.ToString(),
                            data.ToAgent,
                            data.ToPayName,
                            data.ToUserName,
                            data.ToIsoCode,
                            data.ToBankCode,
                            data.ToAccountNo,
                            data.ToAccountDisplayName,
                            data.ToAmount,
                            data.ToFee,
                            data.Reason,
                            data.Remark
                        }),
                        AfterDetailInfo = null
                    };

                    if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParams))
                    {
                        scope.Dispose();

                        return (false, count, "Delete Transfer Info, Create Admin Adjustment Info Log Fail.");
                    }

                    scope.Complete();

                    count = 1;
                }

                message = "Delete Transfer Info Success.";
            }
            else
            {
                return (false, count, "Delete Transfer Info Data Fail.TransferId is null.");
            }

            return (true, count, message);

        }

        #region Private Methods For Set Transfer Info

        /// <summary>
        /// 檢核 共有 資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckSharedParameterForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail,
                                                            out string message)
        {
            message = string.Empty;

            if (bankFinancialTransferInfoDetail == null)
            {
                message = "Data Invalid.";

                return false;
            }

            #region Parameter Values Check

            if (bankFinancialTransferInfoDetail.TransferType == null
                || bankFinancialTransferInfoDetail.TransferType == 0)
            {
                message = "Data-[Transfer Type] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.TransactionType == null
                || bankFinancialTransferInfoDetail.TransactionType == 0)
            {
                message = "Data-[Transaction Type] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.TransDate == null)
            {
                message = "Data-[Trans Date] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.CreateTime == null)
            {
                message = "Data-[CreateTime] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.Creator == null)
            {
                message = "Data-[Creator] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.UpdateTime == null)
            {
                message = "Data-[UpdateTime] invalid.Notify admin please.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.Updator == null)
            {
                message = "Data-[Updator] invalid.Notify admin please.";

                return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// 檢核 加項 和 減項 的資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckParameterForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail,
                                                      out string message)
        {
            message = string.Empty;

            #region From Data

            if (bankFinancialTransferInfoDetail.FromChannel == null || bankFinancialTransferInfoDetail.FromChannel == 0)
            {
                message = "Data-[From Channel] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.PSP &&
                (bankFinancialTransferInfoDetail.FromPayId == null || bankFinancialTransferInfoDetail.FromPayId == 0))
            {
                message = "Data-[From Channel] is PSP.But [From Pay] is invalid.";

                return false;
            }

            if ((bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.HeadOffice) &&
               bankFinancialTransferInfoDetail.FromAgent == null)
            {
                message = "Data-[From Channel] is Local Bank or Head Office.But [From Agent] is invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromSiteId == null)
            {
                message = "Data-[From Site] invalid.";

                return false;

            }

            if (bankFinancialTransferInfoDetail.FromSiteId != 4 &&
                bankFinancialTransferInfoDetail.FromSiteId != 1 &&
                bankFinancialTransferInfoDetail.FromSiteId != 2 &&
                bankFinancialTransferInfoDetail.FromSiteId != 3)
            {
                message = "Data-[From Site] not match.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromSiteName == null)
            {
                message = "Data-[From SiteName] Invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromCurrencyId == null || bankFinancialTransferInfoDetail.FromCurrencyId == 0)
            {
                message = "Data-[From Currency] invalid.";

                return false;
            }

            // if (bankFinancialTransferInfoDetail.FromSysId == null || bankFinancialTransferInfoDetail.FromSysId == 0)
            // {
            //     message = "Data-[From Bank Code] invalid.";
            //
            //     return false;
            // }

            if (bankFinancialTransferInfoDetail.FromAccountNo == null)
            {
                message = "Data-[From Account No.] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromAccountDisplayName == null)
            {
                message = "Data-[From Account Display Name] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromAmount == null || bankFinancialTransferInfoDetail.FromAmount < 0)
            {
                message = "Data-[From Amount] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.FromFee == null || bankFinancialTransferInfoDetail.FromFee < 0)
            {
                message = "Data-[From Fee] invalid.";

                return false;
            }

            #endregion

            #region To Data

            if (bankFinancialTransferInfoDetail.ToChannel == null || bankFinancialTransferInfoDetail.ToChannel == 0)
            {
                message = "Data-[To Channel] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.PSP &&
                (bankFinancialTransferInfoDetail.ToPayId == null || bankFinancialTransferInfoDetail.ToPayId == 0))
            {
                message = "Data-[To Channel] is PSP.But [To Pay] is invalid.";

                return false;
            }

            if ((bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.HeadOffice) &&
               bankFinancialTransferInfoDetail.ToAgent == null)
            {
                message = "Data-[To Channel] is Local Bank or Head Office.But [To Agent] is invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSiteId == null)
            {
                message = "Data-[To Site] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSiteId != 4 &&
                bankFinancialTransferInfoDetail.ToSiteId != 1 &&
                bankFinancialTransferInfoDetail.ToSiteId != 2 &&
                bankFinancialTransferInfoDetail.ToSiteId != 3)
            {
                message = "Data-[To Site] not match.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToSiteName == null)
            {
                message = "Data-[To SiteName] Invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToCurrencyId == null || bankFinancialTransferInfoDetail.ToCurrencyId == 0)
            {
                message = "Data-[To Currency] invalid.";

                return false;
            }

            // if (bankFinancialTransferInfoDetail.ToSysId == null || bankFinancialTransferInfoDetail.ToSysId == 0)
            // {
            //     message = "Data-[To Bank Code] invalid.";
            //
            //     return false;
            // }

            if (bankFinancialTransferInfoDetail.ToAccountNo == null)
            {
                message = "Data-[To Account No.] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToAccountDisplayName == null)
            {
                message = "Data-[To Account Display Name] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToAmount == null || bankFinancialTransferInfoDetail.ToAmount < 0)
            {
                message = "Data-[To Amount] invalid.";

                return false;
            }

            if (bankFinancialTransferInfoDetail.ToFee == null || bankFinancialTransferInfoDetail.ToFee < 0)
            {
                message = "Data-[To Fee] invalid.";

                return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// 檢核 和 判斷 單加項 或 單減項 的資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <param name="transactionType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool CheckParameterWithTypeOtherForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail,
                                                                   out string message)
        {
            message = string.Empty;

            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
            {
                //Minus

                if (bankFinancialTransferInfoDetail.FromChannel == null || bankFinancialTransferInfoDetail.FromChannel == 0)
                {
                    message = "Data-[From Channel] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.PSP &&
               (bankFinancialTransferInfoDetail.FromPayId == null || bankFinancialTransferInfoDetail.FromPayId == 0))
                {
                    message = "Data-[From Channel] is PSP.But [From Pay] is invalid.";

                    return false;
                }

                if ((bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.FromChannel == TransferChannelEnum.HeadOffice) &&
                   bankFinancialTransferInfoDetail.FromAgent == null)
                {
                    message = "Data-[From Channel] is Local Bank or Head Office.But [From Agent] is invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromSiteId == null)
                {
                    message = "Data-[From Site] invalid.";

                    return false;

                }

                if (bankFinancialTransferInfoDetail.FromSiteId != 4 &&
                    bankFinancialTransferInfoDetail.FromSiteId != 1 &&
                    bankFinancialTransferInfoDetail.FromSiteId != 2 &&
                    bankFinancialTransferInfoDetail.FromSiteId != 3)
                {
                    message = "Data-[From Site] not match.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromSiteName == null)
                {
                    message = "Data-[From SiteName] Invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromCurrencyId == null || bankFinancialTransferInfoDetail.FromCurrencyId == 0)
                {
                    message = "Data-[From Currency] invalid.";

                    return false;
                }

                // if (bankFinancialTransferInfoDetail.FromSysId == null || bankFinancialTransferInfoDetail.FromSysId == 0)
                // {
                //     message = "Data-[From Bank Code] invalid.";
                //
                //     return false;
                // }

                if (bankFinancialTransferInfoDetail.FromAccountNo == null)
                {
                    message = "Data-[From Account No.] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromAccountDisplayName == null)
                {
                    message = "Data-[From Account Display Name] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromAmount == null || bankFinancialTransferInfoDetail.FromAmount < 0)
                {
                    message = "Data-[From Amount] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.FromFee == null || bankFinancialTransferInfoDetail.FromFee < 0)
                {
                    message = "Data-[From Fee] invalid.";

                    return false;
                }
            }
            else
            {
                //Plus

                if (bankFinancialTransferInfoDetail.ToChannel == null || bankFinancialTransferInfoDetail.ToChannel == 0)
                {
                    message = "Data-[To Channel] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.PSP &&
                    (bankFinancialTransferInfoDetail.ToPayId == null || bankFinancialTransferInfoDetail.ToPayId == 0))
                {
                    message = "Data-[To Channel] is PSP.But [To Pay] is invalid.";

                    return false;
                }

                if ((bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.LocalBank || bankFinancialTransferInfoDetail.ToChannel == TransferChannelEnum.HeadOffice) &&
                   bankFinancialTransferInfoDetail.ToAgent == null)
                {
                    message = "Data-[To Channel] is Local Bank or Head Office.But [To Agent] is invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSiteId == null)
                {
                    message = "Data-[To Site] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSiteId != 4 &&
                    bankFinancialTransferInfoDetail.ToSiteId != 1 &&
                    bankFinancialTransferInfoDetail.ToSiteId != 2 &&
                    bankFinancialTransferInfoDetail.ToSiteId != 3)
                {
                    message = "Data-[To Site] not match.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToSiteName == null)
                {
                    message = "Data-[To SiteName] Invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToCurrencyId == null || bankFinancialTransferInfoDetail.ToCurrencyId == 0)
                {
                    message = "Data-[To Currency] invalid.";

                    return false;
                }

                // if (bankFinancialTransferInfoDetail.ToSysId == null || bankFinancialTransferInfoDetail.ToSysId == 0)
                // {
                //     message = "Data-[To Bank Code] invalid.";
                //
                //     return false;
                // }

                if (bankFinancialTransferInfoDetail.ToAccountNo == null)
                {
                    message = "Data-[To Account No.] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToAccountDisplayName == null)
                {
                    message = "Data-[To Account Display Name] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToAmount == null || bankFinancialTransferInfoDetail.ToAmount < 0)
                {
                    message = "Data-[To Amount] invalid.";

                    return false;
                }

                if (bankFinancialTransferInfoDetail.ToFee == null || bankFinancialTransferInfoDetail.ToFee < 0)
                {
                    message = "Data-[To Fee] invalid.";

                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// 裝載 加項設定資料 和 減項設定資料
        /// </summary>
        /// <param name="bankFinancialTransferInfoDetail"></param>
        /// <returns></returns>
        private (BankFinancialTransferInfoModel TransferInfoFromModel, BankFinancialTransferInfoModel TransferInfoToModel) SetParameterModelForSetTransferInfo(BankFinancialTransferInfoTransDetailModel bankFinancialTransferInfoDetail)
        {
            var transferInfoFromModel = new BankFinancialTransferInfoModel();
            var transferInfoToModel = new BankFinancialTransferInfoModel();

            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus
                || bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyMinus)
            {
                transferInfoFromModel = new BankFinancialTransferInfoModel
                {
                    TransferId = bankFinancialTransferInfoDetail.TransferId,
                    TransactionType = bankFinancialTransferInfoDetail.TransactionType,
                    TransferType = bankFinancialTransferInfoDetail.TransferType,
                    PlusOrMinus = PlusOrMinusEnum.Minus,
                    TransDate = bankFinancialTransferInfoDetail.TransDate.DateTime,
                    Channel = bankFinancialTransferInfoDetail.FromChannel,
                    Agent = bankFinancialTransferInfoDetail.FromAgent == "" ? null : bankFinancialTransferInfoDetail.FromAgent,
                    PayId = bankFinancialTransferInfoDetail.FromPayId == 0 ? null : bankFinancialTransferInfoDetail.FromPayId,
                    SiteId = bankFinancialTransferInfoDetail.FromSiteId,
                    SiteName = bankFinancialTransferInfoDetail.FromSiteName == "" ? null : bankFinancialTransferInfoDetail.FromSiteName,
                    CustId = bankFinancialTransferInfoDetail.FromCustId == 0 ? null : bankFinancialTransferInfoDetail.FromCustId,
                    CurrencyId = bankFinancialTransferInfoDetail.FromCurrencyId,
                    SysId = bankFinancialTransferInfoDetail.FromSysId,
                    AccountNo = bankFinancialTransferInfoDetail.FromAccountNo,
                    AccountDisplayName = bankFinancialTransferInfoDetail.FromAccountDisplayName,
                    Amount = bankFinancialTransferInfoDetail.FromAmount,
                    Fee = bankFinancialTransferInfoDetail.FromFee,
                    Reason = bankFinancialTransferInfoDetail.Reason,
                    Remark = bankFinancialTransferInfoDetail.Remark,
                    //CreateTime = bankFinancialTransferInfoDetail.CreateTime,
                    Creator = bankFinancialTransferInfoDetail.Creator,
                    //UpdateTime = bankFinancialTransferInfoDetail.UpdateTime,
                    Updator = bankFinancialTransferInfoDetail.Updator
                };
            }
            if (bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.PlusAndMinus
               || bankFinancialTransferInfoDetail.TransactionType == TransactionTypeEnum.OnlyPlus)
            {
                transferInfoToModel = new BankFinancialTransferInfoModel
                {
                    TransferId = bankFinancialTransferInfoDetail.TransferId,
                    TransactionType = bankFinancialTransferInfoDetail.TransactionType,
                    TransferType = bankFinancialTransferInfoDetail.TransferType,
                    PlusOrMinus = PlusOrMinusEnum.Plus,
                    TransDate = bankFinancialTransferInfoDetail.TransDate.DateTime,
                    Channel = bankFinancialTransferInfoDetail.ToChannel,
                    Agent = bankFinancialTransferInfoDetail.ToAgent == "" ? null : bankFinancialTransferInfoDetail.ToAgent,
                    PayId = bankFinancialTransferInfoDetail.ToPayId == 0 ? null : bankFinancialTransferInfoDetail.ToPayId,
                    SiteId = bankFinancialTransferInfoDetail.ToSiteId,
                    SiteName = bankFinancialTransferInfoDetail.ToSiteName == "" ? null : bankFinancialTransferInfoDetail.ToSiteName,
                    CustId = bankFinancialTransferInfoDetail.ToCustId == 0 ? null : bankFinancialTransferInfoDetail.ToCustId,
                    CurrencyId = bankFinancialTransferInfoDetail.ToCurrencyId,
                    SysId = bankFinancialTransferInfoDetail.ToSysId,
                    AccountNo = bankFinancialTransferInfoDetail.ToAccountNo,
                    AccountDisplayName = bankFinancialTransferInfoDetail.ToAccountDisplayName,
                    Amount = bankFinancialTransferInfoDetail.ToAmount,
                    Fee = bankFinancialTransferInfoDetail.ToFee,
                    Reason = bankFinancialTransferInfoDetail.Reason,
                    Remark = bankFinancialTransferInfoDetail.Remark,
                    //CreateTime = bankFinancialTransferInfoDetail.CreateTime,
                    Creator = bankFinancialTransferInfoDetail.Creator,
                    //UpdateTime = bankFinancialTransferInfoDetail.UpdateTime,
                    Updator = bankFinancialTransferInfoDetail.Updator
                };
            }

            return (transferInfoFromModel, transferInfoToModel);

        }

        #endregion

        #region Receipt Logic

        public BankFinancialTransferInfoReceiptModel GetTransferIngoReceipt(string transferId)
        {
            var result = _bankFinancialService.GetTransferInfoReceipt(transferId);

            if (result != null)
            {
                result.PicFileName = _bucketUrl + result.PicFileName;
            }

            return result;
        }

        private async Task<bool> InsertTransferReceipt(IFormFile transferReceipt, string transferId)
        {
            var ext = Path.GetExtension(transferReceipt.FileName);
            var path = $"/transfer/receipt/{transferId}{ext}";
            var key = _folder + path;
            var success = await _awsS3Helper.UploadFileAsync(transferReceipt, _bucketName, key);
            var result = success ? path : string.Empty;

            if (success)
            {
                return _bankFinancialService.SetTransferInfoReceipt(transferId, key) > 0;
            }

            return false;
        }

        public class ProcessedImage
        {
            public string FileName { get; set; }
            public byte[] FileBytes { get; set; }
        }

        private readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> _contentTypesCheck = new Dictionary<string, System.Drawing.Imaging.ImageFormat>(comparer: StringComparer.OrdinalIgnoreCase)
        {
            {"image/png",System.Drawing.Imaging.ImageFormat.Png},
            {"image/jpeg",System.Drawing.Imaging.ImageFormat.Jpeg},
            { "image/gif",System.Drawing.Imaging.ImageFormat.Gif}
        };

        #endregion

        #endregion

        #region Transfer Info Report

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReport(IEnumerable<int> siteIds,
                                                                                       IEnumerable<string> accountNos,
                                                                                       DateTime startDate,
                                                                                       DateTime endDate,
                                                                                       int pageNumber,
                                                                                       int pageSize,
                                                                                       out int totalPages,
                                                                                       out int rowCount)
        {
            return _bankFinancialService.GetTransferInfoReport(JsonConvert.SerializeObject(siteIds),
                                                               JsonConvert.SerializeObject(accountNos),
                                                               startDate,
                                                               endDate,
                                                               (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                                                               Math.Abs(pageSize),
                                                               out totalPages,
                                                               out rowCount);
        }

        public IEnumerable<BankFinancialTransferInfoDetailModel> GetTransferInfoReportForExport(IEnumerable<int> siteIds,
                                                                                  IEnumerable<string> accountNos,
                                                                                  DateTime startDate,
                                                                                  DateTime endDate)
        {
            return _bankFinancialService.GetTransferInfoReportForExport(JsonConvert.SerializeObject(siteIds),
                                                               JsonConvert.SerializeObject(accountNos),
                                                               startDate,
                                                               endDate);
        }

        #endregion

        #region Bank Financial Report

        public IEnumerable<BankFinancialDataModel> GetBankFinancialReport(string siteIds, string currencyIds, string startDate, string endDate, string timeZone)
        {
            var keyValuePairs = new Dictionary<string, BankFinancialDataModel>();

            var accountData = _bankFinancialService.GetTransferAccountList();
            var payData = _paymentManageService.GetPayList();
            var siteData = _reportService.GetServerInfoList();
            var currencyData = _reportService.GetCurrencyList();

            var depositData = _bankFinancialService.GetDepositSummary(siteIds, currencyIds, startDate, endDate, timeZone);
            var withdrawalData = _bankFinancialService.GetWithdrawalSummary(siteIds, currencyIds, startDate, endDate, timeZone);
            var transferData = _bankFinancialService.GetTransferSummary(siteIds, currencyIds, startDate, endDate, timeZone);

            #region Data Combine

            foreach (var data in depositData)
            {
                BankFinancialDataModel bankFinancialDataModel;

                if (data.DepositType == DepositTypeEnum.LocalBanking)
                {
                    var account = accountData.FirstOrDefault(x => x.AccountNo == data.AccountNo);

                    if (account != null)
                    {
                        if (!keyValuePairs.ContainsKey($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                        {
                            bankFinancialDataModel = new BankFinancialDataModel();
                            keyValuePairs.Add($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                            bankFinancialDataModel.SiteId = data.SiteId;
                            bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                            bankFinancialDataModel.CurrencyId = data.CurrencyId;
                            bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                            bankFinancialDataModel.Channel = account.Channel;
                            bankFinancialDataModel.Agent = account.Agent;
                            bankFinancialDataModel.AccountNo = data.AccountNo;
                            bankFinancialDataModel.AccountName = account.DisplayName;
                            bankFinancialDataModel.Purpose = account.Purpose;
                            bankFinancialDataModel.AccountStatus = account.Status;
                        }

                        bankFinancialDataModel = keyValuePairs[$"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];
                        bankFinancialDataModel.DepositAmount += data.DepositSummary;
                    }
                }
                else
                {
                    var account = accountData.FirstOrDefault(x => x.PayId.ToString() == data.AccountNo && x.SiteId == data.SiteId && x.CurrencyId == data.CurrencyId);
                    var pay = payData.FirstOrDefault(x => x.Id.ToString() == data.AccountNo);

                    if (!keyValuePairs.ContainsKey($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                    {
                        var status = pay.Status == 1 ? TransferAccountStatusEnum.Open : TransferAccountStatusEnum.Close;

                        bankFinancialDataModel = new BankFinancialDataModel();
                        keyValuePairs.Add($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                        bankFinancialDataModel.SiteId = data.SiteId;
                        bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                        bankFinancialDataModel.CurrencyId = data.CurrencyId;
                        bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                        bankFinancialDataModel.Channel = TransferChannelEnum.PSP;
                        bankFinancialDataModel.PayId = account?.PayId ?? pay.Id;
                        bankFinancialDataModel.PayName = account == null ? pay.PayName : account.PayName;
                        bankFinancialDataModel.AccountNo = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.AccountNo;
                        bankFinancialDataModel.AccountName = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.DisplayName;
                        bankFinancialDataModel.Purpose = account == null ? "1,2,3" : account.Purpose;
                        bankFinancialDataModel.AccountStatus = account?.Status ?? status;
                    }
                    bankFinancialDataModel = keyValuePairs[$"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];
                    bankFinancialDataModel.DepositAmount += data.DepositSummary;
                }

            }

            foreach (var data in withdrawalData)
            {
                BankFinancialDataModel bankFinancialDataModel;

                if (data.WithdrawalType == WithdrawalTypeEnum.LocalBank)
                {
                    var account = accountData.FirstOrDefault(x => x.AccountNo == data.AccountNo);

                    if (account != null)
                    {
                        if (!keyValuePairs.ContainsKey($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                        {
                            bankFinancialDataModel = new BankFinancialDataModel();
                            keyValuePairs.Add($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                            bankFinancialDataModel.SiteId = data.SiteId;
                            bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                            bankFinancialDataModel.CurrencyId = data.CurrencyId;
                            bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                            bankFinancialDataModel.Channel = account.Channel;
                            bankFinancialDataModel.Agent = account.Agent;
                            bankFinancialDataModel.AccountNo = data.AccountNo;
                            bankFinancialDataModel.AccountName = account.DisplayName;
                            bankFinancialDataModel.Purpose = account.Purpose;
                            bankFinancialDataModel.AccountStatus = account.Status;
                        }

                        bankFinancialDataModel = keyValuePairs[$"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];

                        bankFinancialDataModel.WithdrawalAmount += data.WithdrawalSummary;

                    }
                }
                else if (data.WithdrawalType == WithdrawalTypeEnum.OnlineBanking)
                {
                    var account = accountData.FirstOrDefault(x => x.PayId.ToString() == data.AccountNo && x.SiteId == data.SiteId && x.CurrencyId == data.CurrencyId);
                    var pay = payData.FirstOrDefault(x => x.Id.ToString() == data.AccountNo);

                    if (!keyValuePairs.ContainsKey($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                    {
                        var status = pay.Status == 1 ? TransferAccountStatusEnum.Open : TransferAccountStatusEnum.Close;

                        bankFinancialDataModel = new BankFinancialDataModel();
                        keyValuePairs.Add($"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                        bankFinancialDataModel.SiteId = data.SiteId;
                        bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                        bankFinancialDataModel.CurrencyId = data.CurrencyId;
                        bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                        bankFinancialDataModel.Channel = TransferChannelEnum.PSP;
                        bankFinancialDataModel.PayId = account?.PayId ?? pay.Id;
                        bankFinancialDataModel.PayName = account == null ? pay.PayName : account.PayName;
                        bankFinancialDataModel.AccountNo = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.AccountNo;
                        bankFinancialDataModel.AccountName = account == null ? pay.PayName + " " + bankFinancialDataModel.IsoCode : account.DisplayName;
                        bankFinancialDataModel.Purpose = account == null ? "1,2,3" : account.Purpose;
                        bankFinancialDataModel.AccountStatus = account?.Status ?? status;
                    }

                    bankFinancialDataModel = keyValuePairs[$"{TransferChannelEnum.PSP.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];

                    bankFinancialDataModel.WithdrawalAmount += data.WithdrawalSummary;

                }

            }

            foreach (var data in transferData)
            {
                var account = accountData.FirstOrDefault(x => x.AccountNo == data.AccountNo);

                if (account != null)
                {
                    BankFinancialDataModel bankFinancialDataModel;
                    if (!keyValuePairs.ContainsKey($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"))
                    {
                        bankFinancialDataModel = new BankFinancialDataModel();
                        keyValuePairs.Add($"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}", bankFinancialDataModel);
                        bankFinancialDataModel.SiteId = data.SiteId;
                        bankFinancialDataModel.SiteName = siteData.Where(x => x.SiteId == data.SiteId).Select(x => x.SiteName).FirstOrDefault();
                        bankFinancialDataModel.CurrencyId = data.CurrencyId;
                        bankFinancialDataModel.IsoCode = currencyData.Where(x => x.CurrencyId == data.CurrencyId).Select(x => x.IsoCode).FirstOrDefault();
                        bankFinancialDataModel.Channel = account.Channel;
                        bankFinancialDataModel.Agent = account.Agent;
                        bankFinancialDataModel.PayId = account.PayId;
                        bankFinancialDataModel.PayName = account.PayName;
                        bankFinancialDataModel.AccountNo = data.AccountNo;
                        bankFinancialDataModel.AccountName = account.DisplayName;
                        bankFinancialDataModel.Purpose = account.Purpose;
                        bankFinancialDataModel.AccountStatus = account.Status;
                    }

                    bankFinancialDataModel = keyValuePairs[$"{account.Channel.GetDescription()}-{data.AccountNo}-{data.CurrencyId}"];

                    bankFinancialDataModel.TransferAmount += data.TransferSummary;
                }
            }

            #endregion

            #region 補0

            #endregion

            var Data = keyValuePairs.Select(s => s.Value).OrderBy(value => value.SiteId)
                                                         .ThenBy(value => value.CurrencyId)
                                                         .ThenBy(value => value.Channel)
                                                         .ThenBy(value => value.AccountNo);

            return Data;
        }

        public async Task<IEnumerable<BankFinancialReportCurrencySummary>> GetBankFinancialReportSummaryAsync(
            GetFinancialReportParams input)
        {
            var report = await GetBankFinancialReportWithCacheAsync(input);

            if (!report.Data.Any())
            {
                return Array.Empty<BankFinancialReportCurrencySummary>();
            }

            var accountSummary = GetBankFinancialReportAccountSummaryAsync(report);
            var agentSummary = GetBankFinancialReportAgentSummaryAsync(accountSummary);
            var channelSummary = GetBankFinancialReportChannelSummaryAsync(agentSummary);
            var result = GetBankFinancialReportCurrencySummaryAsync(channelSummary);

            return result;
        }

        private IEnumerable<BankFinancialReportAccountSummary> GetBankFinancialReportAccountSummaryAsync(
            BankFinancialReportResult report)
        {
            var result = report.Data
                .GroupBy(x => new { x.FinancialId, x.AccountNo, x.Agent, x.Channel, x.CurrencyId })
                .Select(g =>
                {
                    var transferTypes = GetTransferTypeToDisplay();
                    var details = transferTypes.ToDictionary(x => x.ToString(), x =>
                    {
                        var sum = g
                            .Where(y => y.TransferType == x)
                            .Sum(y => y.Summary);

                        return sum;
                    });

                    var accountInfo = report.AccountInfoList.FirstOrDefault(x => x.FinancialId == g.Key.FinancialId);
                    var openBalance = accountInfo?.OpenBalance;
                    var netBalance = g.Sum(x => x.Summary);

                    return new BankFinancialReportAccountSummary
                    {
                        CurrencyId = g.Key.CurrencyId,
                        Channel = g.Key.Channel,
                        Agent = g.Key.Agent,
                        Key = accountInfo?.DisplayName ?? g.Key.AccountNo,
                        Details = details,
                        Fees = g.Sum(x => x.FeeSummary),
                        OpenBalance = openBalance,
                        NetBalance = netBalance,
                        ClosingBalance = openBalance + netBalance
                    };
                });

            return result;
        }

        private IEnumerable<BankFinancialReportAgentSummary> GetBankFinancialReportAgentSummaryAsync(
            IEnumerable<BankFinancialReportAccountSummary> accountSummary)
        {
            var result = accountSummary
                .GroupBy(x => new { x.Agent, x.Channel, x.CurrencyId })
                .Select(g =>
                {
                    var transferTypes = GetTransferTypeToDisplay();
                    var details =
                        transferTypes.ToDictionary(x => x.ToString(), x =>
                        {
                            var sum = g.Sum(y => y.Details[x.ToString()]);

                            return sum;
                        });

                    return new BankFinancialReportAgentSummary
                    {
                        CurrencyId = g.Key.CurrencyId,
                        Channel = g.Key.Channel,
                        Key = g.Key.Agent,
                        Details = details,
                        Fees = g.Sum(x => x.Fees),
                        OpenBalance = g.Sum(x => x.OpenBalance),
                        NetBalance = g.Sum(x => x.NetBalance),
                        ClosingBalance = g.Sum(x => x.ClosingBalance),
                        Children = g
                    };
                });

            return result;
        }

        private IEnumerable<BankFinancialReportChannelSummary> GetBankFinancialReportChannelSummaryAsync(
            IEnumerable<BankFinancialReportAgentSummary> agentSummary)
        {
            var result = agentSummary
                .GroupBy(x => new { x.Channel, x.CurrencyId })
                .Select(g =>
                {
                    var transferTypes = GetTransferTypeToDisplay();
                    var details =
                        transferTypes.ToDictionary(x => x.ToString(), x =>
                        {
                            var sum = g.Sum(y => y.Details[x.ToString()]);

                            return sum;
                        });

                    return new BankFinancialReportChannelSummary
                    {
                        CurrencyId = g.Key.CurrencyId,
                        Key = g.Key.Channel.ToString(),
                        Details = details,
                        Fees = g.Sum(x => x.Fees),
                        OpenBalance = g.Sum(x => x.OpenBalance),
                        NetBalance = g.Sum(x => x.NetBalance),
                        ClosingBalance = g.Sum(x => x.ClosingBalance),
                        Children = g
                    };
                });

            return result;
        }

        private IEnumerable<BankFinancialReportCurrencySummary> GetBankFinancialReportCurrencySummaryAsync(
            IEnumerable<BankFinancialReportChannelSummary> channelSummary)
        {
            var gData = channelSummary
                .GroupBy(x => x.CurrencyId);
            var result = gData
                .Select(g =>
                {
                    var transferTypes = GetTransferTypeToDisplay();
                    var details =
                        transferTypes.ToDictionary(x => x.ToString(), x =>
                        {
                            var sum = g.Sum(y => y.Details[x.ToString()]);

                            return sum;
                        });

                    var d = g.First();
                    return new BankFinancialReportCurrencySummary
                    {
                        CurrencyId = g.Key,
                        Key = g.Key.ToString(),
                        Details = details,
                        Fees = g.Sum(x => x.Fees),
                        OpenBalance = g.Sum(x => x.OpenBalance),
                        NetBalance = g.Sum(x => x.NetBalance),
                        ClosingBalance = g.Sum(x => x.ClosingBalance),
                        Children = g
                    };
                });

            return result;
        }

        public async Task<byte[]> ExportBankFinancialReportAsync(GetFinancialReportParams input)
        {
            var report = await GetBankFinancialReportWithCacheAsync(input);

            if (!report.Data.Any())
            {
                return Array.Empty<byte>();
            }

            var transferTypes = GetTransferTypeToDisplay();
            var data = report.Data
                .OrderBy(x => x.CurrencyId)
                .ThenBy(x => x.Channel)
                .ThenBy(x => x.Agent)
                .GroupBy(x => new { x.FinancialId, x.AccountNo, x.Agent, x.Channel, x.CurrencyId })
                .Select(g =>
                {
                    var details = transferTypes.ToDictionary(x => x.ToString(), x =>
                    {
                        var sum = g
                            .Where(y => y.TransferType == x)
                            .Sum(y => y.Summary);

                        return sum.ToString();
                    });

                    var accountInfo = report.AccountInfoList.FirstOrDefault(x => x.FinancialId == g.Key.FinancialId);
                    var openBalance = accountInfo?.OpenBalance;
                    var netBalance = g.Sum(x => x.Summary);
                    var fee = g.Sum(x => x.FeeSummary);

                    var item = new Dictionary<string, string>
                    {
                        {"Open Balance", openBalance.ToString()},
                        {"Net Balance", netBalance.ToString()},
                        {"Closing Balance", (openBalance + netBalance).ToString()},
                        {"Currency", g.Key.CurrencyId.ToString()},
                        {"Channel", g.Key.Channel.ToString()},
                        {"Agent", g.Key.Agent},
                        {"Account Name", accountInfo?.DisplayName ?? g.Key.AccountNo}
                    };

                    item.AddRange(details);
                    item.Add("Fee", fee.ToString());

                    return item;
                });

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("BankFinancialReport");
            sheet.WriteDictionaryListToSheet(data, true);

            await using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }

        private async Task<BankFinancialReportResult> GetBankFinancialReportWithCacheAsync(
            GetFinancialReportParams input)
        {
            var key = $"FinancialReport:{input.SiteId}:{input.CurrencyIds}:{input.Channel}:{input.Agents}" +
                      $":{input.FinancialAccountIds}:{input.StartTime:yyyyMMddHHmmss}:{input.EndTime:yyyyMMddHHmmss}:{input.TimeZone}";

            return await _cacheHelper.TryGetOrCreateAsync(key, TimeSpan.FromSeconds(30),
                () => GetBankFinancialReportAsync(input));
        }

        private async Task<BankFinancialReportResult> GetBankFinancialReportAsync(GetFinancialReportParams input)
        {
            var deposit = await _bankFinancialService.GetFinancialReportDepositAsync(input);
            var withdrawal = await _bankFinancialService.GetFinancialReportWithdrawalAsync(input);
            var transfer = await _bankFinancialService.GetFinancialReportTransferAsync(input);
            var data = deposit.Union(withdrawal).Union(transfer);
            var ids = data.Select(x => x.FinancialId).Distinct();

            var openBalanceList = await _bankFinancialService.GetFinancialAccountOpenBalanceAsync(
                new GetFinancialAccountOpenBalanceParams
                {
                    SiteId = input.SiteId,
                    FinancialIds = JsonConvert.SerializeObject(ids),
                    StartTime = input.StartTime
                });

            return new BankFinancialReportResult
            {
                Data = data,
                AccountInfoList = openBalanceList
            };
        }

        private static IEnumerable<TransferTypeEnum> GetTransferTypeToDisplay()
        {
            var except = new[]
            {
                TransferTypeEnum.Undefined,
                TransferTypeEnum.Deposit,
                TransferTypeEnum.Withdrawal
            };

            var result = EnumExtension.EnumToIEnumerable(except).ToList();
            result.Insert(0, TransferTypeEnum.Deposit);
            result.Insert(1, TransferTypeEnum.Withdrawal);

            return result;
        }

        #endregion
    }
}