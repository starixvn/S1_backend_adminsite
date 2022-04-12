using AdminSite.Setting.Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdminSite.Setting.DataAccessLayer.Interface
{
    internal interface IAdminModuleService
    {
        IEnumerable<AdminModuleModel> GetAll();
    }
}
