using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IEventService
    {
        Task BeginTransaction();
        Task Commit();
        Task RollBack();

        /// <summary>
        /// 取得後台使用者設定的世界盃設定
        /// </summary>
        /// <param name="langId">語系Id，看前端傳什語系</param>
        /// <returns></returns>
        IEnumerable<CupEventSettingDto> GetCupEventSetting(string langId);

        /// <summary>
        /// 取得世界盃國家下拉選單內容
        /// </summary>
        /// <returns></returns>
        IEnumerable<CupEventOptionsDto> GetCupEventOptions(string langId);

        /// <summary>
        /// 更新 Status 狀態
        /// </summary>
        /// <returns></returns>
        Task<int> SetCupEventPrediction();

        /// <summary>
        /// 寫入 CupEventSetting
        /// </summary>
        /// <param name="setData"></param>
        /// <returns></returns>
        Task<int> InsertCupEventPrediction(CupEventSettingDto setData);

        IEnumerable<CupEventMemberReportDto> GetMemberReport(GetCupEventMemberReportRequest request, out int totalPages, out int rowCount);
        Task<IEnumerable<CupEventMemberReportDto>> GetMemberReport(ExportCupEventMemberReportRequest request);
        Task<IEnumerable<CupEventPredictiveStatisticsDto>> GetPredictiveStatistics(int siteId);
        Task<IEnumerable<CupEventPredictDetailDto>> GetPredictDetail(int siteId, int teamId);
        Task<int> SetPayoutDone(int siteId, int teamId, string updateOperator);
        Task<IEnumerable<CupEventPredictScore>> GetCupEventPredictScore();

        #region Lottery

        public IEnumerable<GetLotteryEventRoundData> GetLotteryEventRoundList(int siteId);
        public IEnumerable<LotteryEventResponse> GetLotteryEventSetting(GetLotteryEventRequest request);
        public LotteryEventResponse GetLotteryEventSettingBySq(int sq);
        public Task<int> SetLotteryEventSetting(int sq, DateTime startDate, DateTime endDate, DateTime drawDate, int isSuspend, string excludeSetting,string excludeTagList, string updator);
        public Task<int> AddLotteryEvent(int siteId, DateTime startDate, DateTime endDate, DateTime drawDate, string excludeSetting, string excludeTagList, string updator);
        public LotteryTicketHistory GetLotteryTicketHistory(int siteId, int roundId);
        public IEnumerable<RaffleTicketStickerResponse> GetRaffleTicketStickerSetting(int siteId);
        public Task<int> SetRaffleTicketStickerSetting(SetRaffleTicketStickerRequest request, string updator);
        public Task<GetStickerCountResponse> GetStickerCount(GetStickerCountRequest request);
        public Task<int> SetStickerCount(SetStickerCountRequest request);
        public IEnumerable<EventRestrictionResponse> GetEventRestrictionList(GetEventRestrictionRequest request);
        public Task<int> InsEventRestriction(int siteId, int currencyId, int custId ,string reason ,string creator);
        public Task<int> DelEventRestriction(int custId);
        public Task<GetStickerReportResponse> GetStickerReport(GetStickerReportRequest request);
        public IEnumerable<GetCustLotteryTicket> GetCustLotteryTicket(int siteId, string currencyIds, string userName, int roundId, string ticketNo);

        #endregion
    }
}