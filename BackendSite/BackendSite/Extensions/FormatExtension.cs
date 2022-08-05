using System.Collections.Generic;

namespace BackendSite.Extensions
{
    public static class FormatExtension
    {
        public static IEnumerable<string> SplitRemoveSpace(string str, char separator)
        {
            return string.IsNullOrEmpty(str) 
                ? new string[]{}
                : str.Replace(" ", "").Split(separator);
        }
    }
}