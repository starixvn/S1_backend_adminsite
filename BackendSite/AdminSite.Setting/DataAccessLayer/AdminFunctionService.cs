using AdminSite.Setting.DataAccessLayer.Interface;
using AdminSite.Setting.Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdminSite.Setting.DataAccessLayer
{
    internal class AdminFunctionService : IAdminFunctionService
    {
        private readonly MySqlClient _mainDbClient;
        public AdjustmentInfoLogService(IEnumerable<MySqlClient> dbClient)
        {
            _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
        }

        public IEnumerable<AdminFunctionModel> GetAll()
        {
            return 
        }
    }
}
