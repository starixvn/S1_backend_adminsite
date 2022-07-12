using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IWorldCupPredictionService
    {
        Task BeginTransaction();
        Task Commit();
        Task RollBack();

        /// <summary>
        /// 取得後台使用者設定的世界盃設定
        /// </summary>
        /// <param name="langId">語系Id，看前端傳什語系</param>
        /// <returns></returns>
        IEnumerable<WorldCupPredictionSettingDto> GetWorldCupPredictionSetting(string langId);

        /// <summary>
        /// 取得世界盃國家下拉選單內容
        /// </summary>
        /// <returns></returns>
        IEnumerable<WorldCupPredictionOptionsDto> GetWorldCupPredictionOptions();

        /// <summary>
        /// 更新 Status 狀態
        /// </summary>
        /// <returns></returns>
        Task<int> SetWorldCupPrediction();

        /// <summary>
        /// 寫入 WorldCupEventSetting
        /// </summary>
        /// <param name="setData"></param>
        /// <returns></returns>
        Task<int> InsertWorldCupPrediction(WorldCupPredictionSettingDto setData);

        IEnumerable<WorldCupMemberReportDto> GetMemberReport(GetWorldCupMemberReportRequest request, out int totalPages, out int rowCount);
        Task<IEnumerable<WorldCupMemberReportDto>> GetMemberReport(ExportWorldCupMemberReportRequest request);
        Task<IEnumerable<WorldCupPredictiveStatisticsDto>> GetPredictiveStatistics(int siteId);
        Task<IEnumerable<WorldCupPredictDetailDto>> GetPredictDetail(int siteId, int teamId);
        Task<int> SetPayoutDone(int siteId, int teamId, string updateOperator);
    }
}