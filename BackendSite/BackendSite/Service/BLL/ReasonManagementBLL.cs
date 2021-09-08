using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class ReasonManagementBLL
    {
        private readonly ReasonManagementService reasonManagementService;

        public ReasonManagementBLL(ReasonManagementService reasonManagementService)
        {
            this.reasonManagementService = reasonManagementService;
        }
        public IEnumerable<ReasonManagementFuncKey> GetFuncKey()
        {
            return reasonManagementService.GetFuncKey();
        }

        public GetReasonManagementDataResult GetReasonManagementData(GetReasonManagementDataPara para)
        {
            return reasonManagementService.GetReasonManagementData(para);
        }

        public int SetReasonManagement(SetReasonManagementPara para,int UserId)
        {
            var now = DateTime.UtcNow;
            return reasonManagementService.SetReasonManagement(para, UserId, now);
        }
        public int InsertReasonManagement(InsertReasonManagementPara para, int UserId)
        {
            var now = DateTime.UtcNow;
            return reasonManagementService.InsertReasonManagement(para, UserId, now);
        }
    }
}
