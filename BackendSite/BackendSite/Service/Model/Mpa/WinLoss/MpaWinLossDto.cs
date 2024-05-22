using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Mpa.WinLoss
{
    public class MpaRateDto
    {
        public DateTime RateDate { get; set; }

        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public int TargetCurrencyId { get; set; }

        public string TargetCurrencyName { get; set; }

        public decimal Rate { get; set; }
    }

    public class MpaCurrencyDto
    {
        public int CurrencyId { get; set; }

        public string IsoCode { get; set; }
    }

    public interface IMpaWinLoss : IMpaWinLossSimple
    {
        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal TotalBonus { get; set; }
    }

    public interface IMpaWinLossSimple : IMpaWinLossBase
    {

        public decimal BAPlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public decimal BACommission { get; set; }

    }

    public interface IMpaWinLossBase
    {
        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public decimal Margin { get; set; }
    }

    public class MpaWinLossWithPlayerInfo : MpaWinLoss
    {
        public MpaWinLossWithPlayerInfo() { }

        public MpaWinLossWithPlayerInfo(MpaWinLossGroupByModel m) : base(m)
        {
            UserName = m.UserName;
            SabaId = m.TpUserName;
            CustId = m.CustId;
        }

        public string UserName { get; set; }
        public string CustId { get; set; }

        public string SabaId { get; set; }

        public decimal ClosingBalance { get; set; }
    }

    public class MpaWinLoss : IMpaWinLoss
    {
        public MpaWinLoss() { }

        public MpaWinLoss(MpaWinLossGroupByModel m)
        {
            TurnOver = m.TurnOver;
            ValidTurnOver = m.ValidTurnOver;
            GGR = m.Ggr;
            PlayerWinLoss = m.PlayerWinLoss;
            BAPlayerWinLoss = m.BaPlayerWinLoss;
            ProgressiveWin = m.ProgressiveWin;
            ProgressiveBet = m.ProgressiveBet;
            Commission = m.Commission;
            BACommission = m.BaCommission;
            BetCount = m.BetCount;
            AvgBetAmount = m.AvgBetAmount;
            UniquePlayers = m.UniquePlayers;
            Margin = m.Margin;
            DepositCount = m.DepositCount;
            TotalDepositAmount = m.TotalDepositAmount;
            WithDrawCount = m.WithdrawCount;
            TotalWithDrawAmount = m.TotalWithdrawAmount;
            TotalBonus = m.TotalBonus;
            NetCash = TotalDepositAmount - TotalWithDrawAmount;
        }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal BAPlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public decimal BACommission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public decimal Margin { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal NetCash { get; set; }

        public decimal TotalBonus { get; set; }
    }

    public class MpaWinLossSimple : IMpaWinLossSimple
    {

        public MpaWinLossSimple() { }

        public MpaWinLossSimple(MpaWinLossGroupByModel m)
        {
            TurnOver = m.TurnOver;
            ValidTurnOver = m.ValidTurnOver;
            GGR = m.Ggr;
            PlayerWinLoss = m.PlayerWinLoss;
            BAPlayerWinLoss = m.BaPlayerWinLoss;
            ProgressiveWin = m.ProgressiveWin;
            ProgressiveBet = m.ProgressiveBet;
            Commission = m.Commission;
            BACommission = m.BaCommission;
            BetCount = m.BetCount;
            AvgBetAmount = m.AvgBetAmount;
            UniquePlayers = m.UniquePlayers;
            Margin = m.Margin;
        }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal BAPlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public decimal BACommission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public decimal Margin { get; set; }
    }

    public interface IMpaWinLossDisplay : IMpaWinLossDisplaySimple
    {
        public int DepositCount { get; set; }

        public string TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public string TotalWithDrawAmount { get; set; }

        public string TotalBonus { get; set; }
    }

    public interface IMpaWinLossDisplaySimple : IMpaWinLossBaseDisplaySimple
    {

        public string BAPlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public string BACommission { get; set; }

    }

    public interface IMpaWinLossBaseDisplaySimple
    {
        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }


        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }
    }

    public class MpaWinLossDisplay : IMpaWinLossDisplay
    {
        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string BAPlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public string BACommission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }
        public int DepositCount { get; set; }

        public string TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public string TotalWithDrawAmount { get; set; }

        public string TotalBonus { get; set; }
    }

    public class MpaWinLossDisplaySimple : IMpaWinLossDisplaySimple
    {
        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string BAPlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public string BACommission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }
    }

    public class MpaWinLossBaseDisplaySimple : IMpaWinLossBaseDisplaySimple
    {
        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }
    }


    #region To FrontEnd API Formate

    public class MpaWinLossDetailForFrontEnd<T> where T : IMpaWinLossSimple
    {
        public MpaWinLossDetailForFrontEnd(T data, string key)
        {
            DisplayKey = key;
            Data = data;
        }

        public string DisplayKey { get; set; }

        public T Data { get; set; }
    }

    #endregion


    #region To Excel Export Formate

    public class MpaWinLossByDateDtoExport : MpaWinLossDisplay
    {
        public string Date { get; set; }

        public string Currency { get; set; }
    }

    public class MpaWinLossByTpGameTypeDtoExport : MpaWinLossDisplaySimple
    {
        public string TpGameTypeName { get; set; }

        public string Currency { get; set; }
    }

    public class MpaWinLossByTpCategoryDtoExport : MpaWinLossDisplaySimple
    {
        public string TpCategoryName { get; set; }

        public string Currency { get; set; }
    }

    public class MpaWinLossByPlayerDtoExport : MpaWinLossDisplay
    {
        public string UserName { get; set; }

        public string TpUserName { get; set; }

        public string Currency { get; set; }
    }

    public class MpaWinLossByCurrencyDtoExport : MpaWinLossDisplay
    {
        public string Currency { get; set; }
    }

    public class MpaWinLossByBrandDtoExport : MpaWinLossDisplay
    {
        public string SiteName { get; set; }

        public string Currency { get; set; }
    }

    public class MpaWinLossByAffiliateDtoExport : MpaWinLossDisplay
    {
        public string AffiliateAccount { get; set; }
        public string UserName { get; set; }

        public string TpUserName { get; set; }

        public string Currency { get; set; }
    }


    public class MpaWinLossByDateDownloadDtoExport : MpaWinLossBaseDisplaySimple
    {
        public string Date { get; set; }
        public string Currency { get; set; }
        public string SabaId { get; set; }
        public string UserName { get; set; }

        public string Product { get; set; }

        public string Brand { get; set; }
    }
    #endregion
}
