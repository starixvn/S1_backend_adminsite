using System.Text.RegularExpressions;

using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;
using static BackendSite.Controllers.ApiV2.Finance.Adjustment.BalanceAdjustmentController;

namespace BackendSite.Service.BLL
{
    public class PaymentAdjustmentReportBLL
    {
        private readonly PaymentAdjustmentReportService paymentAdjustmentService;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly ILogger<PaymentAdjustmentReportBLL> _logger;

        public PaymentAdjustmentReportBLL(IServiceProvider provider)
        {
            this.paymentAdjustmentService = provider.GetRequiredService<PaymentAdjustmentReportService>();
            this.serverInfoBLL = provider.GetRequiredService<ServerInfoBLL>();
            this.selLangBLL = provider.GetRequiredService<SelLangBLL>();
            this._bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
        }

        public IEnumerable<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            string userName,
            IEnumerable<int> adjustTypes,
            DateTime startDate,
            DateTime endDate,
            string transId,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount,
            out List<string> currencyAmount)
        {
            var siteList = serverInfoBLL.GetSiteList();
            var getFromAdjustmentInfo = false;
            var AdjustTypesForAI = new List<int>();
            var getFromLogAdminAdjustWallet = false;
            var AdjustTypesForLAAW = new List<int>();

            if (!siteIds.Any())
                siteIds = serverInfoBLL.GetSiteListByLoginUserV2().Select(x => x.SiteId);

            #region AdjustType

            if (!AdjustTypeCheck(adjustTypes,
            out getFromAdjustmentInfo,
            out AdjustTypesForAI,
            out getFromLogAdminAdjustWallet,
            out AdjustTypesForLAAW))
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            #endregion

            var datas = paymentAdjustmentService.GetPaymentAdjustmentReport(
                    JsonConvert.SerializeObject(siteIds),
                    JsonConvert.SerializeObject(currencyIds),
                    getFromAdjustmentInfo,
                    JsonConvert.SerializeObject(AdjustTypesForAI),
                    getFromLogAdminAdjustWallet,
                    JsonConvert.SerializeObject(AdjustTypesForLAAW),
                    userName,
                    startDate,
                    endDate,
                    transId,
                    (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                    Math.Abs(pageSize),
                    out totalPages,
                    out rowCount,
                    out currencyAmount
                );

            var result = new List<PaymentAdjustmentFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
                        SiteId = row.SiteId,
                        CurrencyId = row.CurrencyId,
                        CustId = row.CustId,
                        TransId = row.TransId,
                        CurrencyName = $"{selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}")}{(row.IsTest == 1 ? "(UUS)" : "")}",
                        AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                        AdjustCategory = row.Category,
                        AdjustCategoryName = row.CategoryName,
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        Amount = row.Amount,
                        CreateTime = row.CreateTime,
                        StatusName = row.AdjustType == (int)AdjustTypeEnum.ThirdAdd || row.AdjustType == (int)AdjustTypeEnum.ThirdMinus ? selLangBLL.GetResourceValue($"lbl_AdjustmentStatus2") : selLangBLL.GetResourceValue($"lbl_AdjustmentStatus{row.Status}"),
                        Memo = row.Memo,
                        SabaID = row.SabaID,
                        Operator = row.Operator,
                        WalletType = string.IsNullOrWhiteSpace(row.WalletType) ? string.Empty : row.WalletType
                    });
                }
            }

            return result;
        }

        public async Task<PaymentAdjustmentReportResponseWrapper> GetPaymentAdjustmentReportV2Async(int siteId,
        IEnumerable<int> currencyIds,
        string userName,
        string sabaId,
        string depositId,
        IEnumerable<int> adjustTypes,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string transId,
          IEnumerable<int> memberTypes,
        int pageNumber,
        int pageSize)
        {
            #region AdjustType

            var adjustTypesForAi = new List<int>();
            var adjustTypesForLaaw = new List<int>();

            if (!AdjustTypeCheck(adjustTypes,
            out var getFromAdjustmentInfo,
            out adjustTypesForAi,
            out var getFromLogAdminAdjustWallet,
            out adjustTypesForLaaw))
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            #endregion

            //取 DB 資料
            var queryResult = await paymentAdjustmentService.GetPaymentAdjustmentReportV2Async(
                    JsonConvert.SerializeObject(new List<int>() { siteId }),
                    JsonConvert.SerializeObject(currencyIds),
                    getFromAdjustmentInfo,
                    JsonConvert.SerializeObject(adjustTypesForAi),
                    getFromLogAdminAdjustWallet,
                    JsonConvert.SerializeObject(adjustTypesForLaaw),
                    userName,
                    sabaId,
                    depositId,
                    startDate,
                    endDate,
                    transId,
                    JsonConvert.SerializeObject(memberTypes),
                    (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                    Math.Abs(pageSize));

            var siteList = serverInfoBLL.GetSiteList().ToList();
            var offset = startDate.Offset;
            queryResult.Data = new List<PaymentAdjustmentReportResponse>();

            foreach (var row in queryResult.QueryResult)
            {
                queryResult.Data.Add(ComposePaymentAdjustmentReportResponse(row, siteList, offset));
            }

            return queryResult;
        }



        public List<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentExportReport(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            string userName,
            IEnumerable<int> adjustTypes,
            DateTime startDate,
            DateTime endDate,
            string transId)
        {
            var siteList = serverInfoBLL.GetSiteList();
            var getFromAdjustmentInfo = false;
            var AdjustTypesForAI = new List<int>();
            var getFromLogAdminAdjustWallet = false;
            var AdjustTypesForLAAW = new List<int>();

            if (!siteIds.Any())
                siteIds = serverInfoBLL.GetSiteListByLoginUserV2().Select(x => x.SiteId);

            #region AdjustType

            if (!AdjustTypeCheck(adjustTypes,
            out getFromAdjustmentInfo,
            out AdjustTypesForAI,
            out getFromLogAdminAdjustWallet,
            out AdjustTypesForLAAW))
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            #endregion

            var datas = paymentAdjustmentService.GetPaymentAdjustmentExportReport(
                    JsonConvert.SerializeObject(siteIds),
                    JsonConvert.SerializeObject(currencyIds),
                    getFromAdjustmentInfo,
                    JsonConvert.SerializeObject(AdjustTypesForAI),
                    getFromLogAdminAdjustWallet,
                    JsonConvert.SerializeObject(AdjustTypesForLAAW),
                    userName,
                    startDate,
                    endDate,
                    transId);

            var result = new List<PaymentAdjustmentFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new PaymentAdjustmentFrontEnd
                    {
                        TransId = row.TransId,
                        CurrencyName = $"{selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}")}{(row.IsTest == 1 ? "(UUS)" : "")}",
                        AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                        AdjustCategory = row.Category,
                        AdjustCategoryName = row.CategoryName,
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        SabaID = row.SabaID,
                        Amount = row.Amount,
                        CreateTime = row.CreateTime,
                        StatusName = row.AdjustType == (int)AdjustTypeEnum.ThirdAdd || row.AdjustType == (int)AdjustTypeEnum.ThirdMinus ? selLangBLL.GetResourceValue($"lbl_AdjustmentStatus2") : selLangBLL.GetResourceValue($"lbl_AdjustmentStatus{row.Status}"),
                        Memo = row.Memo,
                        Operator = row.Operator,
                        WalletType = string.IsNullOrWhiteSpace(row.WalletType) ? string.Empty : row.WalletType
                    });
                }
            }

            return result;
        }

        public async Task<List<PaymentAdjustmentReportExport>> GetPaymentAdjustmentExportReportV2(int siteId,
          IEnumerable<int> currencyIds,
          string userName,
          string sabaId,
          string depositId,
          IEnumerable<int> adjustTypes,
          DateTimeOffset startDate,
          DateTimeOffset endDate,
          string transId,
          IEnumerable<int> memberTypes)
        {
            #region AdjustType

            if (!AdjustTypeCheck(adjustTypes,
                    out var getFromAdjustmentInfo,
                    out var AdjustTypesForAI,
                    out var getFromLogAdminAdjustWallet,
                    out var AdjustTypesForLAAW))
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }

            #endregion

            // DB 取資料
            var queryResult = (await paymentAdjustmentService.GetPaymentAdjustmentExportReportV2(
                    JsonConvert.SerializeObject(new List<int> { siteId }),
                    JsonConvert.SerializeObject(currencyIds),
                    getFromAdjustmentInfo,
                    JsonConvert.SerializeObject(AdjustTypesForAI),
                    getFromLogAdminAdjustWallet,
                    JsonConvert.SerializeObject(AdjustTypesForLAAW),
                    userName,
                    sabaId,
                    depositId,
                    startDate,
                    endDate,
                    transId,
                    JsonConvert.SerializeObject(memberTypes))).ToList();


            var result = new List<PaymentAdjustmentReportExport>();
            if (!queryResult.Any()) return result;

            var siteList = serverInfoBLL.GetSiteList();
            var offset = startDate.Offset;

            result.AddRange(queryResult.Select(row => new PaymentAdjustmentReportExport
            {
                TransId = row.TransId,
                CurrencyName = $"{selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}")}{(row.IsTest == 1 ? "(UUS)" : "")}",
                AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                AdjustCategory = row.Category,
                AdjustCategoryName = row.CategoryName,
                SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                UserName = row.UserName,
                SabaID = row.SabaID,
                Amount = row.Amount,
                CreateTime = row.CreateTime.ToOffset(offset).ToString("yyyy-MM-dd HH:mm:ss"),
                StatusName = (row.AdjustType == (int)AdjustTypeEnum.ThirdAdd || row.AdjustType == (int)AdjustTypeEnum.ThirdMinus)
                    ? GetThirdAdjustmentStatusName(row.Status)
                    : GetMailAdjustmentStatusName(row.Status),
                Memo = row.Memo,
                Operator = row.Operator,
                WalletType = string.IsNullOrWhiteSpace(row.WalletType) ? string.Empty : row.WalletType,
                RefId = row.RefId,
                UserType = row.UserType
            }));

            return result;
        }



        public IEnumerable<PaymentAdjustment> GetAdjustmentInfoByCust(int CustId,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount,
            ref List<string> CurrencyAmount)
        {
            return paymentAdjustmentService.GetAdjustmentInfoByCust(CustId, startDate, endDate, Math.Abs(pageSize), (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), out totalPages, out rowCount, ref CurrencyAmount);
        }


        public async Task<PaymentAdjustmentWrapper> GetAdjustmentInfoByCustV2(int siteId,
            int custId,
            int adjustType,
            DateTime? startDate,
            DateTime? endDate,
            int pageSize,
            int pageNumber)
        {
            return await paymentAdjustmentService.GetAdjustmentInfoByCustV2Async(siteId, custId, adjustType, startDate, endDate, Math.Abs(pageSize), (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
        }

        public bool SetAdjustment(ParamsWithLog<SetAdjustment> input)
        {
            var reasonsbefore = _bankFinancialBll.GetReasonByAdjustmentType(new AdjustmentType
            {
                AdjustmentTypeID = (BalanceAdjustmentTypeIDEnum)input.Before.AdjustType,
                MethodTypeID = input.Before.MethodType
            });
            var reasonsafter = _bankFinancialBll.GetReasonByAdjustmentType(new AdjustmentType
            {
                AdjustmentTypeID = (BalanceAdjustmentTypeIDEnum)input.After.AdjustType,
                MethodTypeID = input.After.MethodType
            });

            if (reasonsafter == null || !reasonsafter.Any())
            {
                _logger.LogError("GetReason has no data.");
                return false;
            }
            input.Before.CategoryName = reasonsbefore.FirstOrDefault(x => x.ReasonId == input.Before.Category)?.Description;
            input.After.CategoryName = reasonsafter.FirstOrDefault(x => x.ReasonId == input.After.Category)?.Description;

            var m = Regex.Match(input.TransId, @"\d+");
            int.TryParse(m.Value, out var id);

            // 依 prefix 判斷更新哪張表
            if (input.TransId.StartsWith("a", StringComparison.OrdinalIgnoreCase))
                return paymentAdjustmentService.SetAdjustmentInfo(input, id);

            return paymentAdjustmentService.SetLogAdminAdjustWallet(input, id);
        }

        #region Private Methods

        private bool AdjustTypeCheck(IEnumerable<int> adjustTypes,
            out bool getFromAdjustmentInfo,
            out List<int> AdjustTypesForAI,
            out bool getFromLogAdminAdjustWallet,
            out List<int> AdjustTypesForLAAW)
        {
            getFromAdjustmentInfo = false;
            AdjustTypesForAI = new List<int>();
            getFromLogAdminAdjustWallet = false;
            AdjustTypesForLAAW = new List<int>();

            //先暫時寫死
            //原本是1234抓AdjustmentInfo,56抓LogAdminAdjustWallet
            //但是現在AdjustmentInfo也有56了......後續打算要怎麼規劃呢
            foreach (var adjustType in adjustTypes)
            {
                switch (adjustType)
                {
                    case 1:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(1);
                        break;
                    case 2:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(2);
                        break;
                    case 3:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(3);
                        break;
                    case 4:
                        getFromAdjustmentInfo = true;
                        AdjustTypesForAI.Add(4);
                        break;
                    case 5:
                        getFromLogAdminAdjustWallet = true;
                        AdjustTypesForLAAW.Add(1);
                        break;
                    case 6:
                        getFromLogAdminAdjustWallet = true;
                        AdjustTypesForLAAW.Add(2);
                        break;
                }
            }
            return true;
        }


        private PaymentAdjustmentReportResponse ComposePaymentAdjustmentReportResponse(PaymentAdjustmentV2 row, IEnumerable<ServerInfo> siteList, TimeSpan offset)
        {
            return new PaymentAdjustmentReportResponse
            {
                SiteId = row.SiteId,
                CurrencyId = row.CurrencyId,
                CustId = row.CustId,
                TransId = row.TransId,
                CurrencyName = $"{selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}")}{(row.IsTest == 1 ? "(UUS)" : "")}",
                AdjustTypeName = selLangBLL.GetResourceValue($"lbl_AdjustmentType{row.AdjustType}"),
                AdjustCategory = row.Category,
                AdjustCategoryName = row.CategoryName,
                SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                UserName = row.UserName,
                Amount = row.Amount,
                CreateTime = row.CreateTime,
                StatusName = (row.AdjustType == (int)AdjustTypeEnum.ThirdAdd || row.AdjustType == (int)AdjustTypeEnum.ThirdMinus)
                    ? GetThirdAdjustmentStatusName(row.Status)
                    : GetMailAdjustmentStatusName(row.Status),
                Memo = row.Memo,
                SabaID = row.SabaID,
                Operator = row.Operator,
                WalletType = string.IsNullOrWhiteSpace(row.WalletType) ? string.Empty : row.WalletType,
                CategoryName = row.CategoryName,
                DepositId = row.DepositId,
                AdjustType = row.AdjustType,
                MethodType =(int)row.MethodType
            };
        }

        /// <summary>
        /// 第三方錢包調帳 log 狀態
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string GetThirdAdjustmentStatusName(int status)
        {
            return status switch
            {
                0 => "Success",
                1 => "Fail",
                2 => "Pending ",
                _ => "NotFound Status"
            };
        }


        /// <summary>
        /// 主錢包調帳 log 狀態
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string GetMailAdjustmentStatusName(int status)
        {
            return status switch
            {
                1 => "Pending",
                2 => "Success ",
                3 => "Pending",
                _ => "NotFound Status"
            };
        }

        #endregion
    }
}
