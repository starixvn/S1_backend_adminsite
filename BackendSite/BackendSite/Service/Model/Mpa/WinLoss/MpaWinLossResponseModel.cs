namespace BackendSite.Service.Model.Mpa.WinLoss
{
    public class MpaWinLossResponseForFrontEnd : MpaWinLoss
    {
        public MpaWinLossResponseForFrontEnd(MpaWinLossGroupByModel m, IEnumerable<MpaWinLossDetailForFrontEnd<MpaWinLoss>> children)
            : base(m)
        {
            DisplayKey = m.Key;
            Children = children;
        }
        
        public string DisplayKey { get; set; }

        public IEnumerable<MpaWinLossDetailForFrontEnd<MpaWinLoss>> Children { get; set; }
    }
    
    public class MpaWinLossResponseForFrontEndSimple : MpaWinLossSimple
    {
        public MpaWinLossResponseForFrontEndSimple(MpaWinLossGroupByModel m, IEnumerable<MpaWinLossDetailForFrontEnd<MpaWinLossSimple>> children)
            : base(m)
        {
            DisplayKey = m.Key;
            Children = children;
        }
        
        public string DisplayKey { get; set; }

        public IEnumerable<MpaWinLossDetailForFrontEnd<MpaWinLossSimple>> Children { get; set; }
    }
    
    public class MpaWinLossResponseForFrontEndWithPlayer : MpaWinLossWithPlayerInfo
    {
        public MpaWinLossResponseForFrontEndWithPlayer(MpaWinLossGroupByModel m, IEnumerable<MpaWinLossDetailForFrontEnd<MpaWinLossWithPlayerInfo>> children)
            : base(m)
        {
            DisplayKey = m.Key;
            Children = children;
        }
        
        public string DisplayKey { get; set; }

        public IEnumerable<MpaWinLossDetailForFrontEnd<MpaWinLossWithPlayerInfo>> Children { get; set; }
    }
    
    public class MpaWinLossResponseForFrontEndWithPlayerDetail : MpaWinLossWithPlayerInfo
    {
        public MpaWinLossResponseForFrontEndWithPlayerDetail(MpaWinLossGroupByModel m)
            : base(m)
        {
            DisplayKey = m.Key;
        }
        
        public string DisplayKey { get; set; }
    }
    
    public class MpaWinLossResponseToFrontEndWithPlayerWrapper
    {
        public IEnumerable<MpaWinLossResponseForFrontEndWithPlayer> Data { get; set; }

        public int RowCount { get; set; }
    }
}


