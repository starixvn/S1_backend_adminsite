using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class HotGameData
    {
        public int TpId { get; set; }
        public string GameId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string GameName { get; set; }
        public string GameLogo { get; set; }
        public string TpName { get; set; }
        public string TpLogo { get; set; }
    }

    public class GetHotGameListRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public bool RefreshCache { get; set; }
    }

    public class GetHotGameListDto
    {
        public int Id { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public int TpGameType { get; set; }
        public string GameTypeName { get; set; }
        public int TpCategory { get; set; }
        public string CategoryName { get; set; }
        public string GameLogo { get; set; }
        public string TpLogo { get; set; }
        public int Sort { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string LastUpdateUser { get; set; }
    }

    public class GetHotGameListResponse
    {
        public int Id { get; set; }
        public string TpName { get; set; }
        public string TpGameType { get; set; }
        public string TpCategory { get; set; }
        public string GameName { get; set; }
        public int Sort { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string LastUpdateUser { get; set; }
    }

    public class SetHotGameRequest
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string LangId { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public string GameId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string GameName { get; set; }
        public GameLogo GameLogo { get; set; }
        public string TpLogo { get; set; }
        public int Sort { get; set; }
    }

    public class GameLogo
    {
        public string HotGamePcUrl { get; set; }
        public string HotGameMobileUrl { get; set; }
    }

    public class GetHotGameDetailRequest
    {
        public int Id { get; set; }
    }

    public class GetHotGameDetailDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string GameName { get; set; }
        public string GameId { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int TpGameType { get; set; }
        public string TpCategory { get; set; }
        public string TpCategoryName { get; set; }
        public string TpLogo { get; set; }
        public string GameLogo { get; set; }
    }

    public class GetHotGameDetailResponse
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string GameName { get; set; }
        public string GameId { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public int TpGameType { get; set; }
        public string TpCategory { get; set; }
        public string TpCategoryName { get; set; }
        public string TpLogo { get; set; }
        public string HotGamePcUrl { get; set; }
        public string HotGameMobileUrl { get; set; }
    }

    public class DeleteHotGameRequest
    {
        public int Id { get; set; }
    }

    public class HotGameSortingRequest
    {
        public int Id { get; set; }
        public int Sort { get; set; }
    }


    #region HotGame ApiV2


    #region request

    public class GetTpGameTypeRequest
    {
        public int TpId { get; set; }
    }

    public class GetHotGameOfGameCasinoRequest
    {
        public int TpId { get; set; }
    }

    public class GetHotGameDataListRequestV2
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public HotGameTopCategory TopCategory { get; set; }
        public int TpId { get; set; }
        public int TpGameType { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetHotGameDetailRequestV2
    {
        public int SiteId { get; set; }

        public int Id { get; set; }
    }

    public class SetHotGameRequestV2
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string LangId { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public string GameId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string GameName { get; set; }
        public HotGameTopCategory TopCategory { get; set; }
    }

    public class DeleteHotGameRequestV2
    {
        public int SiteId { get; set; }
        public int Id { get; set; }
    }

    public class HotGameSortingRequestV2
    {
        public int SiteId { get; set; }
        public int Id { get; set; }
        public int Sort { get; set; }
    }

    #endregion

    #region Response


    public class GetHotGameListResponseV2Wrapper
    {
        public IEnumerable<GetHotGameListResponseV2> Data { get; set; }

        public int TotalPages { get; set; }

        public int RowCount { get; set; }
    }

    public class GetHotGameListResponseV2
    {
        public int Id { get; set; }

        /// <summary>
        /// reference Enum => HotGameTopCategory
        /// </summary>
        public string TopCategory { get; set; }

        public string TpName { get; set; }

        public string TpGameType { get; set; }

        public string GameName { get; set; }

        public string GameId { get; set; }

        public int Sort { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string LastUpdateUser { get; set; }
    }

    public class GetHotGameDetailResponseV2
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string GameName { get; set; }
        public string GameId { get; set; }
        public int TpId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string TpCategoryName { get; set; }
        public HotGameTopCategory TopCategory { get; set; }
    }

    #endregion


    #region DTO

    public class HotGameTopCategoryDto
    {
        public int HotGameTopCategoryId { get; set; }

        public string HotGameTopCategoryName { get; set; }

    }

    public class HotGameOfGameCasinoDto
    {
        public string GameId { get; set; }
        public string Name { get; set; }
    }

    public class GetHotGameListDtoV2Wrapper
    {
        public IEnumerable<GetHotGameListDtoV2> Data { get; set; }

        public int TotalPages { get; set; }

        public int RowCount { get; set; }
    }

    public class GetHotGameListDtoV2
    {
        public int Id { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public int TpGameType { get; set; }
        public string GameTypeName { get; set; }
        public int TpCategory { get; set; }
        public string CategoryName { get; set; }
        public int Sort { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string LastUpdateUser { get; set; }
        public HotGameTopCategory TopCategory { get; set; }
    }

    public class GetHotGameDetailDtoV2
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string GameName { get; set; }
        public string GameId { get; set; }
        public int TpId { get; set; }
        public int TpGameType { get; set; }
        public int TpCategory { get; set; }
        public string TpCategoryName { get; set; }
        public HotGameTopCategory TopCategory { get; set; }
    }



    #endregion

    #endregion

}