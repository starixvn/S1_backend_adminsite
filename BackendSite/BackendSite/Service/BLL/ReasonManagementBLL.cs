using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class ReasonManagementBLL
    {
        private readonly ReasonManagementService _reasonManagementService;

        public ReasonManagementBLL(ReasonManagementService reasonManagementService)
        {
            this._reasonManagementService = reasonManagementService;
        }
        public IEnumerable<ReasonManagementFuncKey> GetFuncKey()
        {
            return _reasonManagementService.GetFuncKey();
        }

        public GetReasonManagementDataResult GetReasonManagementData(GetReasonManagementDataPara para)
        {
            return _reasonManagementService.GetReasonManagementData(para);
        }

        public int SetReasonManagement(SetReasonManagementPara para,int userId)
        {
            return _reasonManagementService.SetReasonManagement(para, userId);
        }
        public int InsertReasonManagement(InsertReasonManagementPara para, int userId)
        {
            return _reasonManagementService.InsertReasonManagement(para, userId);
        }
    }
}
