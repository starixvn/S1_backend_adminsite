using System.Net;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackendSite.ActionFilter;

public class TimeoutFilter: ActionFilterAttribute
{
    public int Timeout { get; set; }
    
    public TimeoutFilter(int timeout)
    {
        Timeout = timeout;
    }
    
    public async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
    {
        var controller = actionContext.ControllerContext.Controller;
        var controllerType = controller.GetType();
        var action = controllerType.GetMethod(actionContext.ActionDescriptor.ActionName);
        var tokenSource = new CancellationTokenSource();
        var timeout = TimeoutTask(Timeout);
        object result = null;
        var work = Task.Run(() =>
        {
            result = action.Invoke( controller, actionContext.ActionArguments.Values.ToArray());
        }, tokenSource.Token );
        var finishedTask = await Task.WhenAny(timeout, work);
        if(finishedTask == timeout)
        {
            tokenSource.Cancel( );
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.RequestTimeout);
        }
        else
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
    private static async Task TimeoutTask(int timeoutValue)
    {
        await Task.Delay(timeoutValue);
    }
}