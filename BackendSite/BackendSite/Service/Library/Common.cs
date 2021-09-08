using Microsoft.AspNetCore.Hosting;
using System;
using System.Text.RegularExpressions;

namespace BackendSite.Service.Library
{
    public class Common
    {
        private readonly IWebHostEnvironment environment;
        public Common(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }
        public string CreateSerialId()
        {
            return $"{GetEnv()}{DateTime.Now.ToString("yyMMddhhmmss")}{new Random(Guid.NewGuid().GetHashCode()).Next(10000, 99999)}";
        }
        public string GetEnv()
        {
            if (environment.EnvironmentName == "Development")
            {
                return "D";
            }
            else if (environment.EnvironmentName == "Staging")
            {
                return "S";
            }
            else if (environment.EnvironmentName == "Production")
            {
                return "P";
            }
            else if (environment.EnvironmentName == "QA")
            {
                return "Q";
            }
            return "X";
        }
        public int? TpIdToCustId(string TpId)
        {
            int custId;
            TpId = TpId.Trim();

            if (TpId.IndexOf("_") > -1)
            {
                TpId = TpId.Substring(TpId.LastIndexOf("_"), TpId.Length - TpId.LastIndexOf("_"));
            }
            else if (TpId.IndexOf("x") > -1)
            {
                TpId = TpId.Substring(TpId.LastIndexOf("x"), TpId.Length - TpId.LastIndexOf("x"));
            }

            var m = Regex.Match(TpId, @"\d+");
            var _custId = int.TryParse(m.Value, out custId) ? custId : (int?)null;

            return _custId;
        }
    }
}
