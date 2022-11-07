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
            SetBonusReportRequest(request, out var siteId, out var promotionTypes, out var promotionCategoryIds, out var statuses, out var pageSize, out var startIndex);
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteId);
            p.Add("_BonusCode", request.BonusCode);
            p.Add("_TransId", request.TransId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_UserName", request.UserName);
            p.Add("_PromotionTypes", promotionTypes);
            p.Add("_StartDate", request.StartDate.ToUniversalTime().DateTime);
            p.Add("_EndDate", request.EndDate.ToUniversalTime().DateTime);
            p.Add("_BaseCurrencyId", request.BaseCurrencyId);
            p.Add("_CategoryTypes", promotionCategoryIds);
            p.Add("_Statuses", statuses);

            #region totalAmountSummary

            var currencyTotalAmount = (await _reportDbClient.QueryAsync<TotalAmount>("Admin_BonusInfoTotalAmount_Get_V6", p)).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.Add(new TotalAmount()
                {
                    Currency = "Total",
                    Amount = Math.Round(currencyTotalAmount.Sum(r => r.Amount), 2),
                    Record = currencyTotalAmount.Sum(r => r.Record)
                });
            }

            #endregion

            p.Add("_PageSize", pageSize);
            p.Add("_StartIndex", startIndex);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = new BonusReportDtoWrapper
            {
                Data = await _reportDbClient.QueryAsync<BonusReportDto>("Admin_BonusInfo_Get_V10", p),
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0,
                TotalAmountSummary = currencyTotalAmount
            };

            return result;
        }

        public async Task<IEnumerable<BonusReportDto>> GetBonusReportExportData(BonusReportExportRequestV2 request)
        {
            SetBonusReportExportRequest(request, out var siteIds, out var promotionTypes, out var promotionCategoryIds, out var statuses);

            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_BonusCode", request.BonusCode);
            p.Add("_TransId", request.TransId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_UserName", request.UserName);
            p.Add("_PromotionTypes", promotionTypes);
            p.Add("_StartDate", request.StartDate.ToUniversalTime().DateTime);
            p.Add("_EndDate", request.EndDate.ToUniversalTime().DateTime);
            p.Add("_BaseCurrencyId", request.BaseCurrencyId);
            p.Add("_CategoryTypes", promotionCategoryIds);
            p.Add("_Statuses", statuses);

            return await _reportDbClient.QueryAsync<BonusReportDto>("Admin_BonusInfoExport_Get_V10", p);
        }

        public Task<IEnumerable<PromotionCategoryOptionDto>> GetPromotionCategoryOption()
        {
            return _reportDbClient.QueryAsync<PromotionCategoryOptionDto>("Admin_PromotionCategoryOption_Get");
        }

        #region private method

        /// <summary>
        /// Set Request 參數
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteIds"></param>
        /// <param name="statuses"></param>
        private static void SetBonusReportExportRequest(BonusReportExportRequestV2 request,
            out string siteIds,
            out string promotionTypes,
            out string promotionCategoryIds,
            out string statuses)
        {
            siteIds = JsonConvert.SerializeObject(new List<int>() { request.SiteId });
            promotionTypes = JsonConvert.SerializeObject(request.PromotionTypes.Select(x => (int)x));
            promotionCategoryIds = JsonConvert.SerializeObject(request.PromotionCategoryIds);
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
            out string promotionTypes,
            out string promotionCategoryIds,
            out string statuses,
            out int pageSize,
            out int startIndex)
        {
            siteIds = JsonConvert.SerializeObject(new List<int>() { request.SiteId });
            promotionTypes = JsonConvert.SerializeObject(request.PromotionTypes.Select(x => (int)x));
            promotionCategoryIds = JsonConvert.SerializeObject(request.PromotionCategoryIds);
            statuses = JsonConvert.SerializeObject(request.Statuses);
            pageSize = Math.Abs(request.PageSize);
            startIndex = (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize);
        }

        #endregion
    }
}