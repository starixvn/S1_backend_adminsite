using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

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

        public (bool Success, string Message) SetReasonManagement(SetReasonManagementPara para, int userId)
        {
            var result = _reasonManagementService.SetReasonManagement(para, userId);

            return result switch
            {
                (int)ErrorCodeEnum.Success => (true, string.Empty),
                -1 => (false, "Duplicate description of FuncKey."),
                -3 => (false, $"Id {para.Id} not exists."),
                _ => (false, "Set reason failed.")
            };
        }
        public (bool Success, string Message) InsertReasonManagement(InsertReasonManagementPara para, int userId)
        {
            var result = _reasonManagementService.InsertReasonManagement(para, userId);

            return result switch
            {
                (int)ErrorCodeEnum.Success => (true, string.Empty),
                -1 => (false, "Duplicate description of FuncKey."),
                _ => (false, "Set reason failed.")
            };
        }
    }
}
