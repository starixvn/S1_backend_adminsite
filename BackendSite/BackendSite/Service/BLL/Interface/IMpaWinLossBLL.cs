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
    Task<IEnumerable<MpaWinLossResponseForFrontEnd>> GetMapWinLossResult(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// excel export
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByTpGameTypeDtoExport>> GetMapWinLossByTpGameTypeExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossResponseForFrontEndSimple>> GetMapWinLossResultSimple(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<MpaWinLossResponseToFrontEndWithPlayerWrapper> GetMpaWinLossResultWithPlayerInfo(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    Task<IEnumerable<MpaWinLossResponseForFrontEndWithPlayer>> GetWinLossByPlayerDetail(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

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
    Task<IEnumerable<MpaWinLossByBrandDtoExport>> GetMpaWinLossByBrandExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// api
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByAffiliateDtoExport>> GetMpaWinLossByAffiliateExport(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

    /// <summary>
    /// excel download btn
    /// </summary>
    /// <param name="request"></param>
    /// <param name="groupByType"></param>
    /// <returns></returns>
    Task<IEnumerable<MpaWinLossByDateDownloadDtoExport>> GetMpaWinLossByDateDownload(MpaWinLossRequest request, MpaWinLossGroupByTypeEnum groupByType);

}