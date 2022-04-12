using System;
using System.Collections.Generic;
using System.Text;
using static AdminSite.Setting.Enum.AdminModule;

namespace AdminSite.Setting.Entities.Dtos
{
    internal class AdminModuleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Layer { get; set; }
        public int HeirId { get; set; }
        public int Sort { get; set; }
        public string Translation { get; set; }
        public Status Status { get; set; }
    }
}
