using System;
using System.Collections.Generic;
using System.Text;
using static AdminSite.Setting.Enum.AdminFunction;

namespace AdminSite.Setting.Entities.Dtos
{
    internal class AdminFunctionModel
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public string AdditionalSetting { get; set; }
        public string Translation { get; set; }
        public Status Status { get; set; }
    }
}
