using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class IovationReportBLL : IIovationReportBLL
    {
        private readonly IovationReportService _iovationReport;

        public IovationReportBLL(IovationReportService iovationReport)
        {
            _iovationReport = iovationReport;
        }

        public async Task<IovationReportPageResult<IovationReport>> GetListAsync(IovationReportGetListRequest request)
        {
            var startTime = request.StartTime.ToUniversalTime().DateTime;
            var endTime = request.EndTime.ToUniversalTime().DateTime;
            var deviceType = request.DeviceType == 0 ? null : request.DeviceType.ToString();
            var data = await _iovationReport.GetListAsync(request.CurrencyId, deviceType, request.DeviceId, request.UserName, startTime, endTime, request.PageNumber, request.PageSize);
            var iovationReport = data.IovationReport.Select(a => new IovationReport()
            {
                UserName = a.UserName,
                DeviceId = a.DeviceId,
                Allow = a.Iovation,
                DeviceType = a.DeviceType,
                Score = a.Score,
                Frequency = a.Frequency,
                AssociateAccount = a.AssociateAccount,
                LastUsedTime = new DateTimeOffset(a.UpdateTime, TimeSpan.Zero),
            });

            return new IovationReportPageResult<IovationReport>()
            {
                IovationReport = iovationReport,
                PageNumber = data.PageNumber,
                TotalPages = data.TotalPages,
                RowCount = data.RowCount
            };
        }

        public async Task<IEnumerable<IovationReport>> GetListWithoutPageAsync(IovationReportExportRequest request)
        {
            var startTime = request.StartTime.ToUniversalTime().DateTime;
            var endTime = request.EndTime.ToUniversalTime().DateTime;
            var deviceType = request.DeviceType == 0 ? null : request.DeviceType.ToString();
            var data = await _iovationReport.GetListWithoutPageAsync(request.CurrencyId, deviceType, request.DeviceId, request.UserName, startTime, endTime);
            var iovationReport = data.Select(a => new IovationReport()
            {
                UserName = a.UserName,
                DeviceId = a.DeviceId,
                Allow = a.Iovation,
                DeviceType = a.DeviceType,
                Score = a.Score,
                Frequency = a.Frequency,
                AssociateAccount = a.AssociateAccount,
                LastUsedTime = new DateTimeOffset(a.UpdateTime, TimeSpan.Zero),
            });

            return iovationReport;
        }
    }
}