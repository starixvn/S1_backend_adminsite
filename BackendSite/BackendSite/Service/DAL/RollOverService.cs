using BackendSite.Service.Model;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class RollOverService
    {
        private readonly MySqlClient _mainDbClient;
        public RollOverService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.FirstOrDefault(p => p.DataBase == DataBase.Main);
        }

        public CustRollOver GetCustRollOver(int custId, string bonusCode)
        {
            var result = _mainDbClient.QueryFirst<CustRollOver>("Common_RollOver_Get", new { CustId = custId, BonusCode = bonusCode });
            return result;
        }
    }
}
