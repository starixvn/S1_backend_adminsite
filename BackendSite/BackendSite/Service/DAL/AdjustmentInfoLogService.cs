using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class AdjustmentInfoLogService
    {
        private readonly MySqlClient _mainDbClient;
        public AdjustmentInfoLogService(IEnumerable<MySqlClient> dbClient)
        {
            _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
        }

        public int InsertAdjustmentInfoLog(string menu, int actionType, object param, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("Menu", menu);
            p.Add("ActionType", actionType);
            p.Add("DetailInfo", JsonConvert.SerializeObject(param));
            p.Add("Operator", updateOperator);
            return _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", p);
        }
    }
}