using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class LineChannelService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public LineChannelService(IServiceProvider provider)
        {
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

        public int Set(LineChannelDto data)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", data.SiteId);
            p.Add("_CurrencyId", data.CurrencyId);
            p.Add("_Link", data.Link);
            p.Add("_Status", data.Status);
            p.Add("_Operator", data.LastOperator);

            var result = _mainDbClient.Execute("Admin_LineChannel_Set", p);

            return result;
        }
    }
}
