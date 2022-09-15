using System.Data;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BonusReportService : IBonusReportService
    {
        private readonly MySqlClient _reportDbClient;

        public BonusReportService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public async Task<BonusReportDtoWrapper> GetBonusReportV2Async(BonusReportRequestV2 request)
        {
            SetBonusReportRequest(request, out var siteId, out var statuses, out var pageSize, out var startIndex);

            var p = new DynamicParameters();
            p.Add("_SiteIds", siteId);
            p.Add("_BonusCode", request.BonusCode);
            p.Add("_TransId", request.TransId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_UserName", request.UserName);
            p.Add("_PromotionType", (int)request.PromotionType);
            p.Add("_StartDate", request.StartDate.DateTime);
            p.Add("_EndDate", request.EndDate.DateTime);
            p.Add("_BaseCurrencyId", request.BaseCurrencyId);
            p.Add("_CategoryType", request.PromotionCategoryId);
            p.Add("_Statuses", statuses);

            var tempCurrencyTotalAmount = new List<string>();
            var currencyTotalAmount = (await _reportDbClient.QueryAsync<TotalAmount>("Admin_BonusInfoTotalAmount_Get_V5", p)).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add($"{x.Currency}:{Math.Round(x.Amount, 2)}");
                });
            }

            p.Add("_PageSize", pageSize);
            p.Add("_StartIndex", startIndex);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = new BonusReportDtoWrapper
            {
                Data = await _reportDbClient.QueryAsync<BonusReportDto>("Admin_BonusInfo_Get_V9", p),
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0,
                TempCurrencyTotalAmount = tempCurrencyTotalAmount
            };

            return result;
        }

        public async Task<IEnumerable<BonusReportDto>> GetBonusReportExportData(BonusReportExportRequestV2 request)
        {
            SetBonusReportExportRequest(request, out var siteIds, out var statuses);

            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_BonusCode", request.BonusCode);
            p.Add("_TransId", request.TransId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_UserName", request.UserName);
            p.Add("_PromotionType", (int)request.PromotionType);
            p.Add("_StartDate", request.StartDate.DateTime);
            p.Add("_EndDate", request.EndDate.DateTime);
            p.Add("_BaseCurrencyId", request.BaseCurrencyId);
            p.Add("_CategoryType", request.PromotionCategoryId);
            p.Add("_Statuses", statuses);

            return await _reportDbClient.QueryAsync<BonusReportDto>("Admin_BonusInfoExport_Get_V9", p);
        }

        #region private method

        /// <summary>
        /// Set Request 參數
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteIds"></param>
        /// <param name="statuses"></param>
        private static void SetBonusReportExportRequest(BonusReportExportRequestV2 request, out string siteIds, out string statuses)
        {
            request.Statuses = new List<BonusInfoStatusEnum>()
            {
                BonusInfoStatusEnum.Initial,
                BonusInfoStatusEnum.Running,
                BonusInfoStatusEnum.Canceled,
                BonusInfoStatusEnum.Expiry,
                BonusInfoStatusEnum.Fulfill,
                BonusInfoStatusEnum.TrasferOut
            }.Select(r => (int)r);
            siteIds = JsonConvert.SerializeObject(new List<int>(){ request.SiteId });
            statuses = JsonConvert.SerializeObject(request.Statuses);
        }

        /// <summary>
        /// Set Request 參數
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteIds"></param>
        /// <param name="statuses"></param>
        /// <param name="pageSize"></param>
        /// <param name="startIndex"></param>
        private static void SetBonusReportRequest(BonusReportRequestV2 request,
            out string siteIds,
            out string statuses,
            out int pageSize,
            out int startIndex)
        {
            request.Statuses = new List<BonusInfoStatusEnum>()
            {
                BonusInfoStatusEnum.Initial,
                BonusInfoStatusEnum.Running,
                BonusInfoStatusEnum.Canceled,
                BonusInfoStatusEnum.Expiry,
                BonusInfoStatusEnum.Fulfill,
                BonusInfoStatusEnum.TrasferOut
            }.Select(r => (int)r);
            siteIds = JsonConvert.SerializeObject(new List<int>() { request.SiteId });
            statuses = JsonConvert.SerializeObject(request.Statuses);
            pageSize = Math.Abs(request.PageSize);
            startIndex = (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize);
        }

        #endregion
    }
}