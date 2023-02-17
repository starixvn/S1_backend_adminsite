using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;

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
    Task<IEnumerable<MpaWinLossByDateResponseToFrontEnd>> GetMapWinLossByDateResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByTpGameTypeResponseToFrontEnd>> GetMapWinLossByTpGameTypeResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByTpCategoryResponseToFrontEnd>> GetMapWinLossByTpCategoryResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<MpaWinLossByPlayerResponseToFrontEndWrapper> GetMpaWinLossByPlayerResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);


    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>

    Task<IEnumerable<MpaWinLossByPlayerDetailResponseFrontEnd>> GetMpaWinLossByPlayerDetailResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByCurrencyResponseFrontEnd>> GetMapWinLossByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);
}