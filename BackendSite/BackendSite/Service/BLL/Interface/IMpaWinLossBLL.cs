using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;

namespace BackendSite.Service.BLL.Interface;

public interface IMpaWinLossBLL
{
    /// <summary>
    /// excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByDateDto>> GetMpaWinLossByDateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<Dictionary<string, MpaWinLossByDateResponse>> GetMapWinLossByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// excel export
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByTpGameTypeDto>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<Dictionary<string, MpaWinLossByTpGameTypeResponse>> GetMapWinLossByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// excel export
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByTpCategoryDto>> GetMapWinLossByTpCategoryExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<Dictionary<string, MpaWinLossByTpCategoryResponse>> GetMapWinLossByTpCategoryResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// excel export
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByPlayerDto>> GetMpaWinLossByPlayerExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<Dictionary<string, MpaWinLossByPlayerResponse>> GetMpaWinLossByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);


    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>

    Task<Dictionary<string, MpaWinLossByPlayerDetailResponse>> GetMpaWinLossByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// excel export
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByCurrencyDto>> GetMapWinLossByCurrencyExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<Dictionary<string, MpaWinLossByCurrencyResponse>> GetMapWinLossByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);
}