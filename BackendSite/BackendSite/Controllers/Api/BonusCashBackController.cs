using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusCashBack" })]
    public class BonusCashBackController : Controller
    {
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly BonusCashBackBLL bonusCashBackBLL;

        public BonusCashBackController(ServerInfoBLL serverInfoBLL, BonusCashBackBLL bonusCashBackBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
            this.bonusCashBackBLL = bonusCashBackBLL;
        }

        public ApiResult<CashBackBonus> GetCashBackBonus()
        {
            var result = new ApiResult<CashBackBonus>();
            try
            {
                var serverInfo = serverInfoBLL.GetServerInfo();
                var cashBackBonus = bonusCashBackBLL.GetCashBackBonus(serverInfo.SiteId);

                if (cashBackBonus == null)
                {
                    cashBackBonus = new CashBackBonus();
                    var tpList = bonusCashBackBLL.GetTpList();
                    cashBackBonus.BonusCashBackGames = tpList.Select(x => new CashBackGames() { Id = x.Type, TpName = x.TpName });

                    cashBackBonus.BonusCashBackMinMax = serverInfo.ICurrencyList.Select(x => new CashBackMinMaxAmount() { CurrencyId = x });
                }
                else
                {
                    var resultMinMax = serverInfo.ICurrencyList.Select(x => new CashBackMinMaxAmount() { CurrencyId = x }).ToList();
                    var tmpMinMaxData = cashBackBonus.BonusCashBackMinMax;
                    foreach(var d in tmpMinMaxData)
                    {
                        var get = resultMinMax.Find(x => x.CurrencyId == d.CurrencyId);
                        get.MaxBet = d.MaxBet;
                        get.MinBet = d.MinBet;
                        get.MaxPay = d.MaxPay;
                        get.MinPay = d.MinPay;
                    }

                    cashBackBonus.BonusCashBackMinMax = resultMinMax;
                }

                result.Data = cashBackBonus;
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        public ApiResult<int?> SetCashBackBonus([FromForm]CashBackBonusBase _CashBackBonus, [FromForm]IEnumerable<CashBackGames> _CashBackGames, [FromForm]IEnumerable<CashBackMinMaxAmount> _CashBackMinMaxAmounts)
        {
            var result = new ApiResult<int?>();
            try
            {
                var serverInfo = serverInfoBLL.GetServerInfo();
                if(_CashBackBonus.Id == null)
                {
                    var cashbackId = bonusCashBackBLL.AddCashBackBonus(_CashBackBonus, _CashBackGames, _CashBackMinMaxAmounts, serverInfo.SiteId);
                    result.Data = cashbackId;
                    result.ErrorCode = cashbackId == null ? 21 : 0;
                }
                else
                {
                    bonusCashBackBLL.SetCashBackBonus(_CashBackBonus, _CashBackGames, _CashBackMinMaxAmounts, serverInfo.SiteId);
                    result.Data = _CashBackBonus.Id;
                    result.ErrorCode = 0;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}