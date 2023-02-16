using System.Data;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class MarketingService : IMarketingService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public MarketingService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<PointLevelInfo> GetPointLevelInfos(int siteId, int currencyId)
        {
            return _mainDbClient.Query<PointLevelInfo>("PointLevelInfo_Get", new DynamicParameters(new { SiteId = siteId, CurrencyId = currencyId }));
        }

        public async Task<IEnumerable<PointLevelInfo>> GetPointLevelInfos(int siteId)
        {
            return await _reportDbClient.QueryAsync<PointLevelInfo>("Admin_PointLevelInfo_Get", new DynamicParameters(new { _SiteId = siteId }));
        }

        public bool AddPointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            var param = new
            {
                pointLevelInfo.SiteId,
                pointLevelInfo.CurrencyId,
                pointLevelInfo.PointLevel,
                pointLevelInfo.DepositCnt,
                pointLevelInfo.WithdrawalCnt,
                pointLevelInfo.IsKyc
            };
            return _mainDbClient.Execute("PointLevelInfo_Ins", new DynamicParameters(param)) > 0;
        }

        public bool EditPointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            var param = new
            {
                pointLevelInfo.SiteId,
                pointLevelInfo.CurrencyId,
                pointLevelInfo.PointLevel,
                pointLevelInfo.DepositCnt,
                pointLevelInfo.WithdrawalCnt,
                pointLevelInfo.IsKyc
            };
            return _mainDbClient.Execute("PointLevelInfo_Set", new DynamicParameters(param)) > 0;
        }

        public PointLevelInfo GetPointLevelInfo(int siteId, int currencyId, int point)
        {
            return _mainDbClient.QueryFirst<PointLevelInfo>("PointLevelInfoDetail_Get", new DynamicParameters(new { SiteId = siteId, CurrencyId = currencyId, PointLevel = point }));
        }

        public int DeletePointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            return _mainDbClient.Execute("PointLevelInfo_Del",new { SiteId = pointLevelInfo.SiteId, CurrencyId = pointLevelInfo.CurrencyId, PointLevel = pointLevelInfo.PointLevel });
        }
    }
}