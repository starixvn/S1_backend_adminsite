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
        public List<DashBoardDetailData> TodayData { get; set; }
    }
    public class DashBoardDetailData
    {
        public string ReportName { get; set; }
        public int NoPlayer { get; set; }
        public int NoAdjustmentPlayers { get; set; }
        public decimal NoAdjustmentAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalWithdrawal { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal VaildTurnover { get; set; }
        public decimal GGR { get; set; }
        public decimal Margin { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal NetCash { get; set; }
    }
}
