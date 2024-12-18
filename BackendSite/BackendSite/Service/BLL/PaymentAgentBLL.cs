using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.PaymentAgent;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class PaymentAgentBLL
{
    private readonly PaymentAgentService _paymentAgentService;
    private readonly AdjustmentLogReportService _adjustLogService;
    private readonly ILogger<PaymentAgentBLL> _logger;

    public PaymentAgentBLL(IServiceProvider provider)
    {
        _paymentAgentService = provider.GetRequiredService<PaymentAgentService>();
        _adjustLogService = provider.GetRequiredService<AdjustmentLogReportService>();
    }

    public IEnumerable<PaymentAgentInfo> GetPaymentAgents(IEnumerable<int> siteIds = null,
        IEnumerable<PaymentAgentTypeEnum> types = null,
        IEnumerable<PaymentAgentStatusEnum> status = null)
    {
        var result = _paymentAgentService.GetPaymentAgentList()
            .Where(x => 
                (siteIds == null || x.SiteIds.Intersect(siteIds).Any()) && 
                (types == null || types.Contains(x.Type)) &&
                (status == null || status.Contains(x.Status)));

        return result;
    }

    public bool SetPaymentAgentStatus(int agentId, PaymentAgentStatusEnum status, string @operator)
    {
        var success = _paymentAgentService.SetPaymentAgentStatus(agentId, status);

        #region admin adjustment log

        if (success)
        {
            var oldData = _paymentAgentService
                .GetPaymentAgentList(agentId)
                .First();
            
            _ = _adjustLogService.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                SiteId = 0,
                CurrencyId = 0,
                Operator = @operator,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetPaymentAgent,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    oldData.AgentName,
                    oldData.AgentId,
                    Status = oldData.Status.ToString()
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    Status = status.ToString()
                })
            });
        }

        #endregion
            
        return success;
    }
    
    public bool SetPaymentAgent(SetParams input, string @operator)
    {
        var success = _paymentAgentService.SetPaymentAgent(input);

        #region admin adjustment log

        if (success)
        {
            var isCreate = input.AgentId == 0;
            var oldData = _paymentAgentService
                .GetPaymentAgentList(input.AgentId)
                .First();
            
            _ = _adjustLogService.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                SiteId = 0,
                CurrencyId = 0,
                Operator = @operator,
                ActionType = isCreate ? ActionType.Create : ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetPaymentAgent,
                BeforeDetailInfo = isCreate ? null : JsonConvert.SerializeObject(new
                {
                    oldData.AgentName,
                    oldData.AgentId,
                    oldData.SiteIds,
                    Type = oldData.Type.ToString(),
                    Status = oldData.Status.ToString()
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    input.AgentName,
                    input.AgentId,
                    input.SiteIds,
                    Type = input.Type.ToString(),
                    Status = input.Status.ToString()
                })
            });
        }

        #endregion
        
        return success;
    }
}