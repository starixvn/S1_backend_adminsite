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

namespace BackendSite.Service.DAL
{
    public class TpMaintainService
    {
        private readonly MySqlClient _mainDbClient;

        public TpMaintainService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public IEnumerable<CommonTpInfo> GetCommonTpList()
        {
            return _mainDbClient.Query<CommonTpInfo>("Common_TpList_Get");
        }

        public IEnumerable<TpMaintain> GetTpMaintainList()
        {
            return _mainDbClient.Query<TpMaintain>("Admin_TpMaintain_Get");
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
            return _mainDbClient.Query<TpGameType>("Admin_TpGameType_Get");
        }

        public IEnumerable<TpCategory> GetTPCategoryList()
        {
            return _mainDbClient.Query<TpCategory>("Admin_TPCategory_Get");
        }
    }
}
