using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class BonusCashBackService
    {
        private readonly IConfiguration configuration;
        public BonusCashBackService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<SiteTpList> GetTpList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<SiteTpList>("Admin_TpGameType_Get", commandType: CommandType.StoredProcedure);
        }

        public CashBackBonus GetCashBackBonus(int SiteId)
        {
            CashBackBonus cashBackBonus = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    cashBackBonus = conn.Query<CashBackBonus>("Admin_BonusCashBack_Get", new { SiteId }, tran, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if(cashBackBonus != null)
                    {
                        cashBackBonus.BonusCashBackGames = conn.Query<CashBackGames>("Admin_BonusCashBackGames_Get", new { SiteId, CashbackId = cashBackBonus.Id }, tran, commandType: CommandType.StoredProcedure);
                        cashBackBonus.BonusCashBackMinMax = conn.Query<CashBackMinMaxAmount>("Admin_BonusCashBackMinMax_Get", new { SiteId, CashbackId = cashBackBonus.Id }, tran, commandType: CommandType.StoredProcedure);
                    }

                    tran.Commit();
                }
            };

            return cashBackBonus;
        }

        public int? AddCashBackBonus(CashBackBonusBase _CashBackBonus, IEnumerable<CashBackGames> _CashBackGames,  IEnumerable<CashBackMinMaxAmount> _CashBackMinMaxAmounts, int SiteId)
        {
            int? CashbackId = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    DynamicParameters _props = new DynamicParameters();
                    _props.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    _props.Add("@SiteId", SiteId);
                    _props.Add("@Period", _CashBackBonus.Period);
                    _props.Add("@DayOfWeek", _CashBackBonus.DayOfWeek);
                    _props.Add("@PayMinute", _CashBackBonus.PayMinute);
                    _props.Add("@PayHour", _CashBackBonus.PayHour);
                    _props.Add("@WithdrawCondition", _CashBackBonus.WithdrawCondition);
                    _props.Add("@DefaultPercentage", _CashBackBonus.DefaultPercentage);
                    _props.Add("@TimeZone", _CashBackBonus.TimeZone);

                    conn.Execute("Admin_BonusCashBack_Ins", _props, tran, commandType: CommandType.StoredProcedure);
                    CashbackId = _props.Get<int?>("@Id");

                    if(CashbackId == null)
                    {
                        tran.Rollback();
                    }
                    else
                    {
                        foreach (var game in _CashBackGames)
                        {
                            conn.Execute("Admin_BonusCashBackGames_Set", new { SiteId, CashbackId, TpId = game.Id, game.Percentage, game.Status }, tran, commandType: CommandType.StoredProcedure);
                        }

                        foreach (var amount in _CashBackMinMaxAmounts)
                        {
                            conn.Execute("Admin_BonusCashBackMinMax_Set", new { SiteId, CashbackId, amount.CurrencyId, amount.MinPay, amount.MaxPay, amount.MinBet, amount.MaxBet }, tran, commandType: CommandType.StoredProcedure);
                        }

                        tran.Commit();
                    }
                }
            };

            return CashbackId;
        }

        public void SetCashBackBonus(CashBackBonusBase _CashBackBonus, IEnumerable<CashBackGames> _CashBackGames, IEnumerable<CashBackMinMaxAmount> _CashBackMinMaxAmounts, int SiteId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    DynamicParameters _props = new DynamicParameters();
                    _props.Add("@Id", _CashBackBonus.Id);
                    _props.Add("@SiteId", SiteId);
                    _props.Add("@Period", _CashBackBonus.Period);
                    _props.Add("@DayOfWeek", _CashBackBonus.DayOfWeek);
                    _props.Add("@PayMinute", _CashBackBonus.PayMinute);
                    _props.Add("@PayHour", _CashBackBonus.PayHour);
                    _props.Add("@WithdrawCondition", _CashBackBonus.WithdrawCondition);
                    _props.Add("@DefaultPercentage", _CashBackBonus.DefaultPercentage);
                    _props.Add("@TimeZone", _CashBackBonus.TimeZone);

                    conn.Execute("Admin_BonusCashBack_Set", _props, tran, commandType: CommandType.StoredProcedure);

                    foreach (var game in _CashBackGames)
                    {
                        conn.Execute("Admin_BonusCashBackGames_Set", new { SiteId, CashbackId = _CashBackBonus.Id, TpId = game.Id, game.Percentage, game.Status }, tran, commandType: CommandType.StoredProcedure);
                    }

                    foreach (var amount in _CashBackMinMaxAmounts)
                    {
                        conn.Execute("Admin_BonusCashBackMinMax_Set", new { SiteId, CashbackId = _CashBackBonus.Id, amount.CurrencyId, amount.MinPay, amount.MaxPay, amount.MinBet, amount.MaxBet }, tran, commandType: CommandType.StoredProcedure);
                    }

                    tran.Commit();
                }
            };
        }
    }
}
