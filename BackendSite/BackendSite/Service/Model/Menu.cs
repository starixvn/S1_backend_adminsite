using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class MenuItem
    {
        public bool NoAuth { get; set; }
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
