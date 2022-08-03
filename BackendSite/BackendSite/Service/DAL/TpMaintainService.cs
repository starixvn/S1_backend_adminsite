using System;
using BackendSite.Service.Model.TpMaintain;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using TpCategory = BackendSite.Service.Model.TpMaintain.TpCategory;
using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class TpMaintainService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public TpMaintainService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<CommonTpInfo> GetCommonTpList()
        {
            return _reportDbClient.Query<CommonTpInfo>("Common_TpList_Get").Where(i => i.Id > 0);
        }

        public IEnumerable<TpMaintain> GetTpMaintainList()
        {
            return _reportDbClient.Query<TpMaintain>("Admin_TpMaintain_Get");
        }

        public IEnumerable<TpMaintain> GetTpMaintainListByFilter(string siteIds, int tpId,
            int tpCategoryId, int TpGameType,
            DateTime startDate, DateTime endDate,
            bool isExpired,
           int startIndex, int pageSize,
           out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_TpId", tpId);
            p.Add("_TpCategoryId", tpCategoryId);
            p.Add("_TpGameType", TpGameType);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate);
            p.Add("_IsExpired", isExpired);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var datas = _reportDbClient.Query<TpMaintain>("Admin_TpMaintain_Get_V2", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return datas;
        }

        public bool InsertTpMaintain(TpMaintain input)
        {
            return _mainDbClient.QueryFirst<int>("Admin_TpMaintainV2_Ins", new DynamicParameters(input)) > 0;
        }

        public bool UpdateTpMaintain(TpMaintain input)
        {
            return _mainDbClient.Execute("Admin_TpMaintain_Set", new DynamicParameters(input)) > 0;
        }

        public bool DeleteTpMaintain(int sn)
        {
            return _mainDbClient.Execute("Admin_TpMaintain_Del", new DynamicParameters(new { sn })) > 0;
        }

        public IEnumerable<TpGameType> GetTpGameTypeList()
        {
            return _reportDbClient.Query<TpGameType>("Admin_TpGameType_Get");
        }

        public IEnumerable<TpCategory> GetTPCategoryList()
        {
            return _reportDbClient.Query<TpCategory>("Admin_TPCategory_Get");
        }
    }
}
