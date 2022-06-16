using System.Collections.Generic;

namespace BackendSite.Service.Model.Store.GiftManagement
{
    public class CreateGiftParams
    {
        public CreateGiftParams(CreateGiftRequest req, string userName)
        {
            SiteId = req.SiteId;
            CurrencyId = req.CurrencyId;
            Status = (int) req.Status;
            CategoryId = req.CategoryId;
            Tags = req.Tags;
            Point = req.Point;
            PriceWorth = req.PriceWorth;
            ShowStock = (int) req.ShowStock;
            Stock = req.Stock;
            Sort = req.Sort;
            Infos = req.Infos;
            Operator = userName;
        }

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int CategoryId { get; set; }
        public IEnumerable<int> Tags { get; set; }
        public int Point { get; set; }
        public decimal PriceWorth { get; set; }
        public int ShowStock { get; set; }
        public int Stock { get; set; }
        public int Sort { get; set; }
        public IEnumerable<LangInfoModel> Infos { get; set; }
        public string Operator { get; set; }
    }

    public class UpdateGiftParams
    {
        public UpdateGiftParams(UpdateGiftRequest req, string userName)
        {
            GiftId = req.GiftId;
            Status = (int) req.Status;
            CategoryId = req.CategoryId;
            Tags = req.Tags;
            Point = req.Point;
            PriceWorth = req.PriceWorth;
            ShowStock = (int) req.ShowStock;
            Stock = req.Stock;
            Sort = req.Sort;
            Infos = req.Infos;
            Operator = userName;
        }

        public int GiftId { get; set; }
        public int Status { get; set; }
        public int CategoryId { get; set; }
        public IEnumerable<int> Tags { get; set; }
        public int Point { get; set; }
        public decimal PriceWorth { get; set; }
        public int ShowStock { get; set; }
        public int Stock { get; set; }
        public int Sort { get; set; }
        public IEnumerable<LangInfoModel> Infos { get; set; }
        public string Operator { get; set; }
    }

    public class SetRedemptionLimitParams
    {
        public SetRedemptionLimitParams(SetRedemptionLimitRequest req, string userName)
        {
            SiteId = req.SiteId;
            CurrencyId = req.CurrencyId;
            Status = (int) req.Status;
            Limit = req.Limit;
            Operator = userName;
        }

        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int Limit { get; set; }
        public string Operator { get; set; }
    }
}