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
        private readonly MySqlClient _mainDbClienct;


        public DeviceService(IEnumerable<MySqlClient> sqlCliencts)
        {
            _sqlClienct = sqlCliencts.FirstOrDefault(a => a.DataBase == DataBase.Report);
            _mainDbClienct = sqlCliencts.FirstOrDefault(a => a.DataBase == DataBase.Main);
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
            //因資料量太大 應DBA暫時解需求 將此SP將指向為MainDB *只有此隻SP需要轉MainDB*
            //TODO 後續解完要改回使用ReportDB或者在討論
            return await _mainDbClienct.QueryAsync<CustFPSData>("Admin_CustFPS_Get", p);
        }
    }
}