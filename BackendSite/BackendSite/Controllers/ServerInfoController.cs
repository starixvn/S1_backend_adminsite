using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BackendSite.Controllers
{
    public class ServerInfoController : Controller
    {
        private IWebHostEnvironment environment;
        private IConfiguration configuration;

        public ServerInfoController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }
        public IActionResult Index()
        {
            var ServerInfoData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ServerInfoData.Add("Now", DateTime.Now.ToString());
            ServerInfoData.Add("EnvironmentName", environment.EnvironmentName);
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var i = 1;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerInfoData.Add($"IP {i++}.", ip.ToString());
                }
            }
            ServerInfoData.Add("Url.Host", Request.Host.Host);
            ServerInfoData.Add("Url.Port", Request.Host.Port.ToString());
            var userAgent = Request.Headers["User-Agent"];
            ServerInfoData.Add("User Agent", userAgent);

            var list = new List<string>
            {
                "X-Forwarded-For",
                "LOCAL_ADDR",
                "server_port",
                "Http_Host",
                "HTTP_INCAP_CLIENT_IP",
                "HTTP_TRUE_CLIENT_IP",
                "HTTP_X_REAL_IP",
                "REMOTE_ADDR",
                "HTTP_X_FORWARDED_FOR",
                "HTTP_X_FORWARDED_PROTO",
                "SERVER_NAME"
            };

            foreach (var head in Request.Headers)
            {
                var sKey = head.Key;
                if (list.Contains(sKey, StringComparer.OrdinalIgnoreCase) && !ServerInfoData.ContainsKey(sKey))
                {
                    ServerInfoData.Add(sKey, head.Value);
                }
            }
            return View(ServerInfoData);
        }
    }
}