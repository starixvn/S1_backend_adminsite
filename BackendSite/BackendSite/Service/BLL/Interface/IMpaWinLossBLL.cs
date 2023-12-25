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
    Task<IEnumerable<MpaWinLossByDateDtoExport>> GetMpaWinLossByDateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByTpGameTypeDtoExport>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByTpCategoryDtoExport>> GetMapWinLossByTpCategoryExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByPlayerDtoExport>> GetMpaWinLossByPlayerExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByCurrencyDtoExport>> GetMapWinLossByCurrencyExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByCurrencyResponseFrontEnd>> GetMapWinLossByCurrencyResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByBrandResponseToFrontEnd>> GetMapWinLossByBrandResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);
    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByBrandDtoExport>> GetMpaWinLossByBrandExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);
}