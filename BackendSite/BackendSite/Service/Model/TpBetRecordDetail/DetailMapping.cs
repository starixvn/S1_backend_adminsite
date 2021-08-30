using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpBetRecordDetail.ThirdPartyDataModel;
using System;

namespace BackendSite.Service.Model.TpBetRecordDetail
{
    public class DetailMapping
    {
        public BaseDetail GetDetail(GetDetailObject detailObject)
        {
            try
            {          
                return (TpCategory)detailObject.TpCategoryId switch
                {
                    TpCategory.Sports => GetSports(detailObject),
                    TpCategory.Casino => GetCasino(detailObject),
                    TpCategory.LiveCasino => GetLiveCasino(detailObject),
                    TpCategory.Fishing => GetFishing(detailObject),
                    TpCategory.Cockfighting => GetCockfighting(detailObject),
                    TpCategory.Lottery => GetLottery(detailObject),
                    TpCategory.VirtualSports => GetVirtualSports(detailObject),
                    TpCategory.Poker => GetPoker(detailObject),
                    _ => new BaseDetail(detailObject)
                };
            }
            catch (Exception)
            {
                return new BaseDetail(detailObject);
            }
        }

        private BaseDetail GetSports(GetDetailObject detailObject) 
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                1 => new SabaSportsDetail(detailObject),
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetCasino(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                1 => new SabaCasinoDetail(detailObject),
                14 => new PGCasinoDetail(detailObject),
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetLiveCasino(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetFishing(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetCockfighting(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetLottery(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                1 => new SabaLotteryDetail(detailObject),
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetVirtualSports(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                1 => new SabaVirtualSportsDetail(detailObject),
                _ => new BaseDetail(detailObject)
            };
        }

        private BaseDetail GetPoker(GetDetailObject detailObject)
        {
            return detailObject.TpBetRecordDetail.TpId switch
            {
                _ => new BaseDetail(detailObject)
            };
        }
    }
}
