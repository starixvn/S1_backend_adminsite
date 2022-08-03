using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Extensions;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class LineChannelService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public LineChannelService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<LineChannelDto> GetList(bool isList,int id,int siteId, 
            int startIndex, int pageSize,
            out int totalPages, out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_IsList", isList);
            props.Add("_Id", id);
            props.Add("_SiteId", siteId);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<LineChannelDto>("Admin_LineChannel_Get", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public bool Set(ParamsWithLog<SetLineChannel> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var p = new DynamicParameters();
            p.Add("_Id", input.After.Id);
            p.Add("_SiteId", input.SiteId);
            p.Add("_CurrencyId", input.CurrencyId);
            p.Add("_Link", input.After.Link);
            p.Add("_Status", input.After.Status);
            p.Add("_Operator", input.Operator);

            var isAdd = input.After.Id == 0;
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.LineDepositChannel);
            logParam.Add("_ActionType", (int) (isAdd ? ActionType.Create : ActionType.Update));
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", isAdd ? null : input.Before.ToLog(), DbType.String);
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);
            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_LineChannel_Set", p, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

            return true;
        }

        public bool Delete(ParamsWithLog<DeleteLineChannel> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            
            var p = new DynamicParameters();
            p.Add("_Id", input.Before.Id);
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.LineDepositChannel);
            logParam.Add("_ActionType", (int) ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_LineChannel_Del", p, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

            return true;
        }
    }
}
