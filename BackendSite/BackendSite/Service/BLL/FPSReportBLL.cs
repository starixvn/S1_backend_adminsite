using System.Net;
using System.Text.RegularExpressions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class FPSReportBLL :　IFPSReportBLL 
    {
        private readonly CustInfoBLL _custInfoBLL;
        private readonly IFPSReportService _fpsReportService;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly BlockService _blockService;
        private readonly CourierAdapter _courierAdapter;
        private readonly Dictionary<int, FPSServicesConfig> _fpsServicesConfig;
        private readonly ILogger<FPSReportBLL> _logger;

        public FPSReportBLL(IServiceProvider provider)
        {
            _custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            _fpsReportService = provider.GetRequiredService<IFPSReportService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _blockService = provider.GetRequiredService<BlockService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _fpsServicesConfig = provider.GetRequiredService<ConfigBLL>().GetConfigValuesBySiteId<FPSServicesConfig>("GetFPS");
            _logger = provider.GetRequiredService<ILogger<FPSReportBLL>>();
        }
        
        public async Task<FPSReportPageResult<FPSReport>> GetFPSListAsync(FPSReportGetListRequest request)
        {
            var startTime = request.StartTime.ToUniversalTime().DateTime;
            var endTime = request.EndTime.ToUniversalTime().DateTime;
            var deviceType = request.DeviceType == 0 ? "" : request.DeviceType.ToString().Replace("_", " ");
            var siteList = _serverInfoBll.GetSiteList();
            var data = await _fpsReportService.GetFPSListAsync(request.SiteIds,request.CurrencyIds, deviceType, request.DeviceId, request.UserName, startTime, endTime, request.PageNumber, request.PageSize);
            
            
            foreach (var item in data.FPSDataReport)
            {
                var custInfo = _custInfoBLL.GetCustInfoWithCache(item.CustId);
                var sabaId = $"D{custInfo.SiteName}_{custInfo.CustId}";
                
                var getFPSAssociate = await FPSGetAssociationByAccount(item.SiteId, sabaId);
                var deviceUser = GetDeviceUser(getFPSAssociate.associatedAccountDetails);
                var associatedDevice = GetAssociatedDevices(getFPSAssociate.associatedDeviceDetails);
                
                item.AssociateCount = getFPSAssociate.associatedAccounts;
                item.AssociateDevices = getFPSAssociate.associatedDevices;
                item.DeviceUser = deviceUser;
                item.AssociatedDevice = associatedDevice;
                
            }
            
            var fpsReport = data.FPSDataReport.Select(x => new FPSReport()
            {
                SiteId = x.SiteId,
                SiteName = siteList.FirstOrDefault(x => x.SiteId == x.SiteId).SiteName,
                CustId = x.CustId,
                UserName = x.UserName,
                IsoCode = x.IsoCode,
                Action = x.Action,
                DeviceId = x.DeviceId,
                RiskLevel = x.RiskLevel,
                Browser = x.Browser,
                DeviceType = x.DeviceType,
                IPAddress = x.IPAddress,
                RobotTracking = x.RobotTracking,
                AssociateDevices = x.AssociateDevices,
                AssociateCount = x.AssociateCount,
                AssociatedDevice = x.AssociatedDevice,
                DeviceUser = x.DeviceUser,
                LastUsedTime = new DateTimeOffset(x.UpdateTime, TimeSpan.Zero),
            }).OrderByDescending(x => x.LastUsedTime);

            return new FPSReportPageResult<FPSReport>()
            {
                FPSDataReport = fpsReport,
                TotalPages = data.TotalPages,
                RowCount = data.RowCount
            };
        }

        public async Task<IEnumerable<FPSReport>> GetFPSListWithoutPageAsync(FPSReportExportRequest request)
        {
            var startTime = request.StartTime.ToUniversalTime().DateTime;
            var endTime = request.EndTime.ToUniversalTime().DateTime;
            var deviceType = request.DeviceType == 0 ? null : request.DeviceType.ToString();
            var siteList = _serverInfoBll.GetSiteList();
            var data = await _fpsReportService.GetFPSListWithoutPageAsync(request.SiteIds,request.CurrencyIds, deviceType, request.DeviceId, request.UserName, startTime, endTime);
            
            foreach (var item in data)
            {
                var custInfo = _custInfoBLL.GetCustInfoWithCache(item.CustId);
                var sabaId = $"D{custInfo.SiteName}_{custInfo.CustId}";

                var getFPSAssociate = await FPSGetAssociationByAccount(item.SiteId, sabaId);
                
                var deviceUser = GetDeviceUser(getFPSAssociate.associatedAccountDetails);
                var associatedDevice = GetAssociatedDevices(getFPSAssociate.associatedDeviceDetails);
                
                item.AssociateCount = getFPSAssociate.associatedAccounts;
                item.AssociateDevices = getFPSAssociate.associatedDevices;
                item.DeviceUser = deviceUser;
                item.AssociatedDevice = associatedDevice;
            }


            var fpsReport = data.Select(x => new FPSReport()
            {
                SiteId = x.SiteId,
                SiteName = siteList.FirstOrDefault(x => x.SiteId == x.SiteId).SiteName,
                CustId = x.CustId,
                UserName = x.UserName,
                IsoCode = x.IsoCode,
                Action = x.Action,
                DeviceId = x.DeviceId,
                RiskLevel = x.RiskLevel,
                Browser = x.Browser,
                DeviceType = x.DeviceType,
                IPAddress = x.IPAddress,
                RobotTracking = x.RobotTracking,
                AssociateDevices = x.AssociateDevices,
                AssociateCount = x.AssociateCount,
                AssociatedDevice = x.AssociatedDevice,
                DeviceUser = x.DeviceUser,
                LastUsedTime = new DateTimeOffset(x.UpdateTime, TimeSpan.Zero),
            });

            return fpsReport;
        }

        public async Task<IEnumerable<DeviceUser>> GetFPSAssociateAccount(string deviceId)
        {
            return await _blockService.GetFPSAssociatedList(deviceId);
        }

        public async Task<IEnumerable<AssociatedDevice>> GetFPSAssociateDevices(int custId)
        {
            var custInfo = _custInfoBLL.GetCustInfoWithCache(custId);
            var sabaId = $"D{custInfo.SiteName}_{custInfo.CustId}";

            var getFPSAssociate = await FPSGetAssociationByAccount(custInfo.SiteId, sabaId);

            return getFPSAssociate.associatedDeviceDetails.Select(item => new AssociatedDevice { DeviceId = item.DeviceId, }).ToList();
            //改打API 但保留原方式
            //return await _blockService.GetFPSAssociatedDevicesList(custId);
        }

        public async Task<FPSGetAssociationByAccountResponse> FPSGetAssociationByAccount(int siteId,string userName)
        {
            var config = _fpsServicesConfig[siteId];
            var url = $"{config.ApiUrl}/api/GetAssociationByAccount/{config.Subscriber}/{userName}";
            var headerParams = new Dictionary<string, string>
            {
                { "Authorization", config.Token },
            };

            using var result = await _courierAdapter.GetAsync(url,headerParams,"application/json");
            if (result.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"GetFPSData httpResponse error URL : {url}  StatusCode: {result.StatusCode}");
                return null;
            }

            var contentResult = await result.Content.ReadAsStringAsync();
            var resultResp = JsonConvert.DeserializeObject<FPSGetAssociationByAccountResponse>(contentResult);

            return resultResp;
        }

        public List<DeviceUser> GetDeviceUser(IEnumerable<FPSAssociatedAccountDetail> accountDetails)
        {
            return accountDetails.Select(detail => new DeviceUser()
                { CustId = FilterCustId(detail.UserName), UserName = detail.UserName }).ToList();
        }

        public List<AssociatedDevice> GetAssociatedDevices(IEnumerable<FPSAssociatedDeviceDetail> associatedDeviceDetails)
        {
            return associatedDeviceDetails.Select(detail => new AssociatedDevice() { DeviceId = detail.DeviceId })
                .ToList();
        }

        private static int FilterCustId(string accountName)
        {
            var regex = new Regex(@"_(\d+)$");
            var match = regex.Match(accountName);
            var custIdStr = match.Groups[1].Value;
            int.TryParse(custIdStr, out var custId);
            return custId;
        }
        
    }
}