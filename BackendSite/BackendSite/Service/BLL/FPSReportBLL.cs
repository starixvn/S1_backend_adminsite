using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class FPSReportBLL :　IFPSReportBLL 
    {
        private readonly IFPSReportService _fpsReportService;
        private readonly BlockService _blockService;

        public FPSReportBLL(IServiceProvider provider)
        {
            _fpsReportService = provider.GetRequiredService<IFPSReportService>();
            _blockService = provider.GetRequiredService<BlockService>();
        }
        
        public async Task<FPSReportPageResult<FPSReport>> GetFPSListAsync(FPSReportGetListRequest request)
        {
            var startTime = request.StartTime.ToUniversalTime().DateTime;
            var endTime = request.EndTime.ToUniversalTime().DateTime;
            var deviceType = request.DeviceType == 0 ? null : request.DeviceType.ToString();
            var data = await _fpsReportService.GetFPSListAsync(request.SiteIds,request.CurrencyIds, deviceType, request.DeviceId, request.UserName, startTime, endTime, request.PageNumber, request.PageSize);
            var fpsReport = data.FPSDataReport.Select(x => new FPSReport()
            {
                SiteId = x.SiteId,
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
                LastUsedTime = new DateTimeOffset(x.UpdateTime, TimeSpan.Zero),
            });

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
            var data = await _fpsReportService.GetFPSListWithoutPageAsync(request.SiteIds,request.CurrencyIds, deviceType, request.DeviceId, request.UserName, startTime, endTime);
            var fpsReport = data.Select(x => new FPSReport()
            {
                SiteId = x.SiteId,
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
            return await _blockService.GetFPSAssociatedDevicesList(custId);
        }
    }
}