using System.Data;

using BackendSite.Extensions;
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
        private readonly AdminBLL _adminBll;
        private readonly Tools _tools;
        private readonly PermissionBLL _permissionBll;

        public OTPBLL(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _newAuthorizeBLL = provider.GetService<NewAuthorizeBLL>();
            _adminBll = provider.GetService<AdminBLL>();
            _tools = provider.GetService<Tools>();
            _permissionBll = provider.GetService<PermissionBLL>();
        }

        public IEnumerable<OTPLog> GetOTPLogList(GetOTPLogListRequest request, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var (StartIndex, PageSize) = request.PageNumber.GetStartIndex(request.PageSize);
            var authData = _newAuthorizeBLL.GetAuthData();
            var loginUser = _adminBll.GetUserV2(authData.UserId,authData.SiteId);
            var hasPermission = _permissionBll.CheckPermission(loginUser, 131, 39);

            var p = new
            {
                SiteId = _newAuthorizeBLL.CheckBrand(),
                SelectCurrency = JsonConvert.SerializeObject(request.CurrencyList),
                OTPProvider = request.OTPProvider == OTPProvider.ALL ? string.Empty : request.OTPProvider.ToString(),
                OTPStatus = (int)request.OTPStatus,
                UserName = string.IsNullOrWhiteSpace(request.UserName) ? string.Empty : request.UserName,
                PhoneNo = string.IsNullOrWhiteSpace(request.PhoneNo) ? string.Empty : request.PhoneNo,
                StartDate = request.StartDate.ToUniversalTime().DateTime,
                EndDate = request.EndDate.ToUniversalTime().DateTime,
                PageSize,
                StartIndex
            };

            var props = new DynamicParameters(p);
            props.Add("RowCount", DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<OTPLog>("Admin_OTPLog_Get", props);
            if (!hasPermission)
            {
                foreach (var item in result) 
                {
                    item.PhoneNo = _tools.MaskPhone(item.PhoneNo);
                }
            }

            var totalCount = props.Get<int?>("RowCount") ?? 0;

            pageInfo.SetPageInfo(totalCount, request.PageNumber, request.PageSize);
            return result;
        }



    }
}
