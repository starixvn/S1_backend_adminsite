using System.Text.RegularExpressions;
using System.Transactions;
using Amazon.S3.Model;
using BackendSite.Controllers.ApiV2.Finance.Deposit;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class DepositBLL
    {
        private readonly DepositService depositService;
        private readonly ServerInfoService serverInfoService;
        private readonly DepositAccountingService _depositAccountingService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly CustInfoBLL custInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly Common common;
        private readonly CustInfoService custInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly ICacheHelper _cacheHelper;
        private readonly string payApiUrl;
        private readonly ILogger<DepositBLL> _logger;
        private readonly string _awsDepositFolder;
        private readonly string _awsDepositBucketName;
        private readonly AWSS3Helper _s3Helper;


        public DepositBLL(IServiceProvider provider)
        {
            depositService = provider.GetRequiredService<DepositService>();
            serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _depositAccountingService = provider.GetRequiredService<DepositAccountingService>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _depositAccountingService = provider.GetRequiredService<DepositAccountingService>();
            custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            common = provider.GetRequiredService<Common>();
            custInfoService = provider.GetRequiredService<CustInfoService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            payApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").PayService;
            _awsDepositFolder = configBll.GetConfigValue("AWSDepositFolderName");
            _awsDepositBucketName = configBll.GetConfigValue("AWSPaymentBucketName");
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _logger = provider.GetRequiredService<ILogger<DepositBLL>>();
            _depositAccountingService = provider.GetRequiredService<DepositAccountingService>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, IEnumerable<string> selectBankCode,
            IEnumerable<int> selectCalCode,
            DateTime startDate,
            DateTime endDate,
            DepositDateTypeEnum dateType,
            string depositId,
            IEnumerable<string> userNames,
            int agent,
            IEnumerable<int> currencyId,
            IEnumerable<int> status,
            IEnumerable<int> levelStatus,
            int pageSize,
            decimal startAmount,
            decimal endAmount,
            IEnumerable<int> payIds,
            bool hasReceipt,
            int pageNumber, out int totalPages, out int rowCount, ref List<TotalAmount> currencyAmountRecord)
        {
            return depositService.GetDepositRequestList(selectSite, depositType, selectBankCode, selectCalCode,
                startDate, endDate, dateType, depositId, userNames, agent, currencyId, status, levelStatus, 
                startAmount, endAmount, payIds, hasReceipt,
                pageSize, pageNumber, out totalPages, out rowCount, ref currencyAmountRecord);
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId,
            string TransId,
            int? Status,
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
            return depositService.GetDepositListByCust(CustId,
                TransId,
                Status,
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

        public async Task<int> CreateDeposit(DepositDetailInfo deposit, IFormFile receipt)
        {
            var depositInfo = new DepositInfo3_1
            {
                AccountName = deposit.AccountHolderName,
                CurrencyId = deposit.CurrencyId,
                AccountNo = deposit.AccountNumber,
                SysId = deposit.SysId,
                Ver = 1
            };
            deposit.DepositInfo = JsonConvert.SerializeObject(depositInfo);

            var custId = (string.IsNullOrEmpty(deposit.SabaId)) ? null : common.TpIdToCustId(deposit.SabaId);
            var customer = custInfoBLL.GetCustInfoDetails(custId, deposit.UserName, deposit.SiteId);
            if (customer == null)
            {
                return -1;
            }
            deposit.CurrencyId = customer.CurrencyId;
            deposit.CustId = customer.CustId;
            deposit.UserName = customer.UserName;

            var creator = authorizeBLL.GetAuthData();
            deposit.Operator = creator.UserName;


            var transId = depositService.InsertDepositRequest(deposit);

            if (!string.IsNullOrEmpty(transId) && receipt != null)
            {
                using var memoryStream = new MemoryStream();
                await receipt.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(bytes);

                await SetDepositReceipt(deposit.SiteId, deposit.CustId, base64String, transId, deposit.Operator);
            }

            if (!string.IsNullOrEmpty(transId))
            {
                return 1;
            }
            return 0;
        }


        public async Task<int> SetDepositReceipt(int siteId, int custId, string inputReceipt, string transId, string creator)
        {
            var receipts = GetDepositReceipt(transId);
            var receiptNo = receipts.Any() ? receipts.Max(x => x.ReceiptNo) + 1 : 1;

            var x = inputReceipt.Replace("data:image/png;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");
            var bytes = Convert.FromBase64String(x);
            var stream = new MemoryStream(bytes);

            var putRequest = new PutObjectRequest();
            var key = $"{_awsDepositFolder}/{transId}_{receiptNo}.png";
            putRequest.BucketName = _awsDepositBucketName; // Your bucket name
            putRequest.ContentType = "image/png";
            putRequest.InputStream = stream;
            putRequest.Key = key;
            await _s3Helper.PutObjectAsync(putRequest);

            var fileName = $"{transId}_{receiptNo}";
            var result = await InsertDepositReceipt(siteId, custId, transId, receiptNo, fileName, creator);

            return result;
        }
        public async Task<int> DelDepositReceiptPic(int siteId, int custId, string transId, string picFileName, int receiptNo, string Operator)
        {
            var depositId = ValidateDepositReceiptPic(transId);
            return await depositService.DelDepositReceiptPic(siteId, custId, depositId, picFileName, receiptNo, Operator);
        }

        public async Task<int> InsertDepositReceipt(int siteId, int custId, string transId, int receiptNo, string fileName, string creator)
        {
            var depositId = ValidateDepositReceiptPic(transId);
            return await depositService.SetDepositReceiptPic(siteId, custId, depositId, receiptNo, fileName, creator);
        }

        public int CheckDepositReceipt(int siteId, string transId, bool status, string Operator)
        {
            var depositId = ValidateDepositReceiptPic(transId);
            return depositService.CheckDepositReceipt(siteId, depositId, status, Operator);
        }

        private int ValidateDepositReceiptPic(string transId)
        {
            var depositId = 0;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositId;
        }

        public DepositRequest GetDepositRequest(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            _ = int.TryParse(m.Value, out var depositId);
            return depositService.GetDepositRequest(depositId);
        }

        public GetDepositAccountingListModel GetDepositAccountingList(
            IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            DateTime startDate,
            DateTime endDate,
            string transId,
            string userName,
            int pageNumber,
            int pageSize)
        {
            var result = _depositAccountingService.GetDepositAccountingList(
                JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIds),
                startDate,
                endDate,
                string.IsNullOrEmpty(transId) ? null : transId,
                string.IsNullOrEmpty(userName) ? null : userName,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out var totalPages,
                out var rowCount);

            return new GetDepositAccountingListModel()
            {
                Datas = result.ToList(),
                PageNumber = Math.Abs(pageNumber),
                RowCount = rowCount,
                TotalPages = totalPages
            };
        }

        public DepositAccountingModel GetDepositAccountingByTransId(string transId)
        {
            return _depositAccountingService.GetDepositAccountingByTransId(transId);
        }

        public int EditDepositAccounting(ParamsWithLog<DepositAccountingController.EditRequest> input)
        {
            return _depositAccountingService.EditDepositAccounting(input);
        }

        public IEnumerable<DepositAccountingModel> ExportDepositAccountingList(
            string siteIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            string transId,
            string userName)
        {
            var result = _depositAccountingService.ExportDepositAccountingList(
                siteIds,
                currencyIds,
                startDate,
                endDate,
                string.IsNullOrEmpty(transId) ? null : transId,
                string.IsNullOrEmpty(userName) ? null : userName);

            return result;
        }

        public IEnumerable<DepositComments> GetDepositRequestComment(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositRequestComment(depositId, depositRequest.SiteId);
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment, string Operator, DepositCalCodeSetRequest calCodeInfo , string depositInfo = null)
        {
            var forStatus = new List<int> { (int)DepositStatusEnum.Pending };
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Approved,
                depositRequest, calCodeInfo, CommonExtension.CanRewardPoint(depositRequest.SiteId, depositRequest.CurrencyId), depositInfo);
        }

        public int DepositReject(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> { (int)DepositStatusEnum.Pending };

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Rejected, null, null);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator, DepositCalCodeSetRequest calCodeInfo)
        {
            var forStatus = new List<int>
            {
                (int) DepositStatusEnum.Draft,
                (int) DepositStatusEnum.Pending,
                (int) DepositStatusEnum.Paid,
                (int) DepositStatusEnum.Approved,
                (int) DepositStatusEnum.Rejected,
                (int) DepositStatusEnum.Canceled,
                (int) DepositStatusEnum.Expired,
                (int) DepositStatusEnum.OnHold,
                (int) DepositStatusEnum.Error
            };

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Draft, null, calCodeInfo);
        }
        public int DepositOnHold(string transId, decimal actualDeposit, string comment, string Operator, DepositCalCodeSetRequest calCodeInfo)
        {
            var forStatus = new List<int> { (int)DepositStatusEnum.Pending };
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.OnHold, null, calCodeInfo);
        }
        public int DepositPending(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> { (int)DepositStatusEnum.OnHold };
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Pending, null, null);
        }
        public IEnumerable<DepositReceipt> GetDepositReceipt(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositReceipt(depositId, depositRequest.SiteId);
        }
        public DepositReceipt GetDepositReceiptByNo(string transId, int receiptNo)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositReceiptByNo(depositId, receiptNo, depositRequest.SiteId);
        }

        public IEnumerable<DepositMessage> GetDepositMessageByDepositIds(IEnumerable<int> depositIds)
        {
            return depositService.GetDepositMessage(depositIds);
        }

        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string selectBankCode, string selectCalCode, string depositType, DateTime startDate, DateTime endDate, DepositDateTypeEnum dateType, string depositId, IEnumerable<string> userNames, int agent, string currencyId, string status, string levelStatus, decimal startAmount, decimal endAmount, string payIds)
        {
            return depositService.GetDepositExportList(selectSite, selectBankCode, selectCalCode, depositType, startDate, endDate, dateType, depositId, userNames, agent, currencyId, status, levelStatus, startAmount, endAmount, payIds);
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType) => depositService.GetDepositLimitCurrencyList(siteId, depositType);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(IEnumerable<int> siteId, IEnumerable<int> depositType, IEnumerable<int> currencyId, int pageNumber, int pageSize, out int totalPages, out int rowCount)
            => depositService.GetDepositLimitInfos(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(depositType),
                JsonConvert.SerializeObject(currencyId),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int currencyId)
            => depositService.GetDepositLimitInfos(siteId, currencyId);

        public bool AddDepositLimit(ParamsWithLog<DepositLimitModel> input)
        {
            return depositService.AddDepositLimitInfo(input);
        }

        public bool AddDepositLimit(ParamsWithLog<IEnumerable<DepositLimitModel>> input)
        {
            return depositService.AddDepositLimitInfo(input);
        }

        public bool EditDepositLimit(ParamsWithLog<DepositLimitModel> input)
        {
            return depositService.EditDepositLimitInfo(input);
        }

        public bool EditDepositLimit(ParamsWithLog<IEnumerable<DepositLimitModel>> input)
        {
            var result = depositService.EditDepositLimitInfo(input);

            if (result)
            {
                _cacheHelper.KeyDelete($"DepositLimitInfoList:{input.SiteId}", RedisDataBase.Member);
            }

            return result;
        }

        public DepositLimitInfo GetDepositLimitInfo(int siteId, int depositType, int currencyId, int vipLevel)
        {
            var currentUpperPoint = depositService.GetDepositLimitInfo(siteId, depositType, currencyId, vipLevel);
            return currentUpperPoint;
        }

        public bool DeleteDepositLimit(ParamsWithLog<DepositLimitModel> input)
        {
            var currentUpperPoint =
                depositService.GetDepositLimitInfo(input.SiteId, (int)input.Before.DepositType, input.CurrencyId, input.Before.VipLevel + 1);

            if (currentUpperPoint != null)
                return false;

            return depositService.DeleteDepositLimitInfo(input);
        }

        public bool DeleteDepositLimitV2(ParamsWithLog<DepositLimitModel> input)
        {
            var result = depositService.DeleteDepositLimitInfoV2(input);

            if (result)
            {
                _cacheHelper.KeyDelete($"DepositLimitInfoList:{input.SiteId}", RedisDataBase.Member);
            }

            return result;
        }

        public IEnumerable<DepositTimeList> DepositTimeReportGet(DepositTimeReportPara para,
            out int totalPages, out int rowCount, out List<TotalAmount> currencyTotalAmount)
        {
            var result = depositService.DepositTimeReportGet(JsonConvert.SerializeObject(para.SiteIds),
                JsonConvert.SerializeObject(para.CurrencyId),
                JsonConvert.SerializeObject(para.Status),
                para.StartDate.DateTime,
                para.EndDate.DateTime,
                para.DepositId,
                para.CustId,
                (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize),
                Math.Abs(para.PageSize),
                out totalPages,
                out rowCount,
                out currencyTotalAmount);

            if (result.Any())
            {
                result.ToList().ForEach(x =>
                {
                    var tsC = new TimeSpan(x.CreateTime.Ticks);
                    var tsU = new TimeSpan(x.UpdateTime.Ticks);
                    var ts = tsC.Subtract(tsU).Duration();
                    x.CompleteTime = GetTimeString(ts);

                    if (x.ApprovedTime > DateTime.Now.AddYears(-1000))
                    {
                        var tsA = new TimeSpan(x.ApprovedTime.Value.Ticks);
                        var ts2 = tsC.Subtract(tsA).Duration();
                        x.ToApprovedTime = GetTimeString(ts2);
                    }
                });
            }

            return result;
        }

        public async Task<IEnumerable<DepositTimeExportData>> DepositTimeExportGet(string selectSite, string depositId, DateTime startDate, DateTime endDate, string statuses, string currencyIds, string memberId)
        {
            var exportDataToExcel = new List<DepositTimeExportData>();
            int custId = 0;
            if (!string.IsNullOrWhiteSpace(memberId))
            {
                var custSimpleInfo = await custInfoBLL.GetCustId(null, memberId, JsonConvert.DeserializeObject<List<int>>(selectSite).FirstOrDefault());
                if (custSimpleInfo != null)
                    custId = custSimpleInfo.CustId;
                else
                    return exportDataToExcel;
            }

            var result = depositService.GetExportData(selectSite, depositId, startDate, endDate, statuses, currencyIds, custId);

            result?.ForEach(x =>
            {
                var ts1 = new TimeSpan(x.CreateTime.Ticks);
                var ts2 = new TimeSpan(x.UpdateTime.Ticks);
                var ts = ts1.Subtract(ts2).Duration();

                if (x.ApprovedTime > DateTime.Now.AddYears(-1000))
                {
                    var tsA = new TimeSpan(x.ApprovedTime.Value.Ticks);
                    var ts3 = ts1.Subtract(tsA).Duration();
                    x.ToApprovedTime = GetTimeString(ts3);
                }

                exportDataToExcel.Add(new DepositTimeExportData
                {
                    SiteName = x.SiteName,
                    DepositId = x.DepositId,
                    DepositType = x.DepositTypeName,
                    UserName = x.UserName,
                    Currency = x.IsoCode,
                    Status = x.StatusName,
                    CreateTime = x.CreateTime.DateTime,
                    UpdateTime = x.UpdateTime.DateTime,
                    CompleteTime = GetTimeString(ts),
                    ToApprovedTime = x.ToApprovedTime,
                    EndFlowTime = x.ApprovedTime,
                    ApprovedBy = x.ApprovedBy,
                    StatementTime = x.StatementTime,
                    Operator = x.Operator,
                    SendMessage = string.IsNullOrEmpty(x.DepositMessage) ? string.Empty : GetSendMessage(JsonConvert.DeserializeObject<int[]>(x.DepositMessage))
                });
            });
            return exportDataToExcel;
        }
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


        public int SetDepositRequestAmount(ParamsWithLog<SetAmountRequest> input, int custId)
        {
            var result = 1;
            using var scope = new TransactionScope();

            if (!depositService.SetDepositRequestAmount(input))
            {
                scope.Dispose();
                return 0;
            }

            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = input.Operator,
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                RefTransId = input.TransId,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetDepositRequestAmount,
                BeforeDetailInfo = JsonConvert.SerializeObject(input.Before),
                AfterDetailInfo = JsonConvert.SerializeObject(input.After)
            };

            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                scope.Dispose();
                return 0;
            }

            var comment = $"Set Deposit Amount ,Change {input.Before.Amount} to {input.After.Amount}";

            var m = Regex.Match(input.TransId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var logResult = depositService.SetDepositLog(depositId, input.SiteId, comment, (int)DepositStatusEnum.SetDepositAmount, input.After.Amount, input.Operator);

            if (logResult < 1)
            {
                scope.Dispose();
                return 0;
            }

            if (!custInfoService.AddCustNote(custId, comment, input.Operator))
            {
                scope.Dispose();
                return 0;
            }

            scope.Complete();
            return result;
        }

        public async Task<ApiResult<DepositManualResponse>> DepositManual(DepositManualRequest depositManualRequest)
        {
            var url = $"{payApiUrl}DepositManual";
            var response = await _courierAdapter.MyPostAsync<DepositManualResponse, DepositManualRequest>(url, depositManualRequest);
            return response;
        }

        /// <summary>
        /// 取得 paymentGateWay
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PaymentGateWayOptions> GetPaymentGateWayOptions()
        {
            return depositService.GetPaymentGateWayOptions();
        }

        public int SetDepositMessage(ParamsWithLog<DepositMessageRequest> input)
        {
            var result = 1;

            if (!depositService.SetDepositMessage(input))
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
                MenuEnum = AdminAdjustmentLogMenuEnum.SetDepositMessage,
                BeforeDetailInfo = null,
                AfterDetailInfo = JsonConvert.SerializeObject(input.After)
            };

            _adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam);
            depositService.SetDepositLog(input.After.DepositId, input.SiteId, $"SendMessage:{input.After.StatusName}", (int)input.After.Action, input.After.ActualDeposit, input.Operator);

            return result;
        }

        private string GetSendMessage(int[] depositMessages)
        {
            List<string> depositMessageList = new List<string>();
            depositMessages.ToList().ForEach(x =>
            {
                depositMessageList.Add(((DepositMessageStatus)x).ToString());
            });
            return String.Join(", ", depositMessageList.ToArray());
        }
    }
}
