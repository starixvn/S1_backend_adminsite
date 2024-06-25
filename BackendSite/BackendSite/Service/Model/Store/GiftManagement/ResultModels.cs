using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Store.GiftManagement
{
    public class UploadPictureResult
    {
        public UploadPictureResult(string picUrl)
        {
            PictureUrl = picUrl;
        }
        public string PictureUrl { get; set; }
    }

    public class RedemptionLimit : LastUpdateModel
    {
        public int Limit { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }

    public class GiftInfo
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int GiftId { get; set; }
        public string GiftName { get; set; }
        public int CategoryId { get; set; }
        public string Lang { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public GeneralStatusEnum Status { get; set; }
        public IEnumerable<int> TagIds { get; set; }
        public int Point { get; set; }
        public decimal PriceWorth { get; set; }
        public bool ShowStock { get; set; }
        public int Stock { get; set; }
        public int Sort { get; set; }
        public int Round { get; set; }
        public int GiftType { get; set; }
    }

    public class ProvideGameListDto
    {
        public int SlotId { get; set; }
        public int TpId { get; set; }
        public string TpName { get; set; }
        public string GameName { get; set; }
        public string GameId { get; set; }
    }

    public class ProvideGameList
    {
        public int TpId { get; set; }

        public string TpName { get; set; }
        public IEnumerable<ProvideGameListDto> data { get; set; }
    }
}