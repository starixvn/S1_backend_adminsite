using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IHotEventService
    {
        /// <summary>
        /// 取得聯賽標題下拉清單
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SportLeagueTitleOptionResponse>> GetSportLeagueTitleOptionAsync();

        /// <summary>
        /// 取得聯賽下拉清單
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SportLeaguesOptionResponse>> GetSportLeaguesAsync();

        /// <summary>
        /// 根據聯賽Id，取得該聯賽隊伍下拉清單
        /// </summary>
        /// <param name="sportLeaguesId">聯賽Id</param>
        /// <returns></returns>
        Task<IEnumerable<SportTeamOptionResponse>> GetSportTeamAsync(int sportLeaguesId);

        /// <summary>
        /// 取得 HotEvent 清單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HotEventDataListDtoWrapper> GetHotEventDataLisAsync(HotEventDataListRequest request);

        /// <summary>
        /// 取得 HotEventDetail 內容
        /// </summary>
        /// <param name="id">SQ</param>
        /// <returns></returns>
        Task<HotEventDetailDto> GetHotEventDetailAsync(int id);

        /// <summary>
        /// 建立HotEvent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="createUser"></param>
        /// <returns></returns>
        Task CreateHotEventAsync(CreateHotEventRequest request, string createUser);

        /// <summary>
        /// 修改 HotEvent Data
        /// </summary>
        /// <param name="request"></param>
        /// <param name="updateUser"></param>
        /// <returns></returns>
        Task SetHotEventDataAsync(SetHotEventDataRequest request, string updateUser);

        /// <summary>
        /// 修改 HotEvent 狀態
        /// </summary>
        /// <param name="request"></param>
        /// <param name="updateUser"></param>
        /// <returns></returns>
        Task SetHotEventStatusAsync(SetHotEventStatusRequest request, string updateUser);

        /// <summary>
        /// 刪除HotEvent
        /// </summary>
        /// <param name="id">SQ</param>
        /// <returns></returns>
        Task DeleteHotEventAsync(int id);
    }
}
