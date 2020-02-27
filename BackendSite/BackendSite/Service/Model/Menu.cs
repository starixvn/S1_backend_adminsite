using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class MenuItem
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public List<MenuItem> Childs { get; set; }
        public string FuncKey { get; set; }
    }
}
