using System;
using System.Collections.Generic;
using System.Text;

namespace AdminSite.Setting.DataAccessLayer._base
{
    internal struct StoredProcedures
    {
        internal struct AdminModule
        {
            internal const string GetList = "Admin_AdminModule_GetAll";
        }
        internal struct AdminFunction
        {
            internal const string GetList = "Admin_AdminFunction_GetAll";
        }
        internal struct AdminPermission
        {
            internal const string GetList = "Admin_AdminPermission_GetAll";
        }
    }
}
