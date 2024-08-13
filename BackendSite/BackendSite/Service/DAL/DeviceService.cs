using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class DeviceService : IDeviceService
    {
        private readonly MySqlClient _sqlClienct;

        public DeviceService(IEnumerable<MySqlClient> sqlCliencts)
        {
            _sqlClienct = sqlCliencts.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<CustIovationData>> GetCustIovationList(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _sqlClienct.QueryAsync<CustIovationData>("Admin_CustIovation_Get", p);
        }

        public async Task<IEnumerable<CustShieldRiskInfoData>> GetCustShieldRiskInfoList(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _sqlClienct.QueryAsync<CustShieldRiskInfoData>("Admin_CustShieldRiskInfo_Get", p);
        }

        public async Task<IEnumerable<CustFPSData>> GetCustFPSList(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _sqlClienct.QueryAsync<CustFPSData>("Admin_CustFPS_Get", p);
        }
    }
}