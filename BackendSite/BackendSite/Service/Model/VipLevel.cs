using System;

namespace BackendSite.Service.Model
{
    public class VipLevelInfo
    {
        public string VipLevel { get; set; }
        public int Manually { get; set; }
        public DateTimeOffset QualifiedTime { get; set; }

    }
    public class VipLevelLog
    {
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }
        public string Memo { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Updaters { get; set; }
    }
    public class VipLevelSetting
    {
        public int Level { get; set; }
        public string LevelName { get; set; }
    }
}
