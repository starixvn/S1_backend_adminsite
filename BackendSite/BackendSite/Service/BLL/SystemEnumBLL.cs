using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class SystemEnumBLL
    {
     
        public IEnumerable<int> GetEnumItemByEnumName(string enumName)
        {
            var result=new List<int>();
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
            var getType = allTypes.FirstOrDefault(t => string.Equals(t.Name, enumName, StringComparison.OrdinalIgnoreCase));
            if (getType != null)
            {
                if (getType.Namespace == "BackendSite.Service.Model.enums" && getType.IsEnum)
                {
                    result = Array.ConvertAll((int[])Enum.GetValues(getType), Convert.ToInt32).ToList();
                }
            }
            return result;
        }
    }
}
