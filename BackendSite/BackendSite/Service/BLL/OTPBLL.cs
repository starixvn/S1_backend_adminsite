using System.Data;

using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Dapper;
using Newtonsoft.Json;


namespace BackendSite.Service.BLL
{
    public class OTPBLL
    {
        private readonly MySqlClient _reportDbClient;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;

        public OTPBLL(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _newAuthorizeBLL = provider.GetService<NewAuthorizeBLL>();
        }

        public IEnumerable<OTPLog> GetOTPLogList(GetOTPLogListRequest request, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var p = new 
            {
                SiteId = _newAuthorizeBLL.CheckBrand(),
                SelectCurrency = JsonConvert.SerializeObject(request.CurrencyList),
                OTPProvider = request.OTPProvider.ToString(),
                OTPStatus = (int)request.OTPStatus,
                request.UserName,
                request.PhoneNo,
                StartDate = request.StartDate.ToUniversalTime().DateTime,
                EndDate = request.EndDate.ToUniversalTime().DateTime,
                request.PageSize,
                StartIndex = 1
            };
            var props = new DynamicParameters(request);
            props.Add("RowCount", DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<OTPLog>("Admin_OTPLog_Get", p);
            var totalCount = props.Get<int?>("RowCount") ?? 0;
            
            pageInfo.SetPageInfo(totalCount, request.PageNumber, request.PageSize);
            return result;
        }

        

    }
}
