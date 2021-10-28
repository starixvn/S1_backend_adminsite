using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BackendSite.Extensions
{
    public static class MethodExtension
    {
        public static string GetCurrentMethod()
        {
            return new StackTrace().GetFrame(1)?.GetMethod()?.Name;
        }

        public static string GetCurrentAsyncMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
