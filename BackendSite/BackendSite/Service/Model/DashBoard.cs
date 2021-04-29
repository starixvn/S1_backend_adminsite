using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class DashBoard
    {
    }
    public class DashBoardData
    {
        public int OnlinePlayers { get; set; }
        public int TodayNewRegPlayers { get; set; }
        public int TotalRegPlayers { get; set; }
        public int PendingDeposit { get; set; }
        public int PendingWithdrawal { get; set; }
    }
}
