using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    /// <summary>
    /// 資料回傳結構
    /// </summary>
    public class DepositReportModel
    {
        /// <summary>
        /// 總表帳戶資料
        /// </summary>
        public List<DepositAccountModel> SummaryAccountData
        {
            get;
            set;
        } =new List<DepositAccountModel>();

        /// <summary>
        /// 總表各幣別資料
        /// </summary>
        public List<DepositSummaryReportModel> SummaryCurrencyData
        {
            get;
            set;
        } = new List<DepositSummaryReportModel>();

        /// <summary>
        /// 總表全部資料
        /// </summary>
        public DepositSummaryReportModel SummaryTotalData
        {
            get;
            set;
        } = new DepositSummaryReportModel();

        /// <summary>
        /// 明細加總資料
        /// </summary>
        public DepositSummaryReportModel DateTotalData
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 帳戶資料
    /// </summary>
    public class DepositAccountModel
    {

        public int AccountType
        {
            get;
            set;
        }

        public string AccountId
        {
            get;
            set;
        }

        public string AccountName
        {
            get;
            set;
        }

        public List<DepositSummaryReportModel> SummaryData
        {
            get;
            set;
        } = new List<DepositSummaryReportModel>();

        public DepositSummaryReportModel SummaryTotalData
        {
            get;
            set;
        } = new DepositSummaryReportModel();

        public List<DepositByDateReportModel> DateData
        {
            get;
            set;
        } = new List<DepositByDateReportModel>();

        public DepositByDateReportModel DateTotalData
        {
            get;
            set;
        } = new DepositByDateReportModel();

    }

    #region For Combine Account Data

    public class DepositSummaryReportModel
    {

        public int CurrencyId
        {
            get;
            set;
        }

        public string IsoCode
        {
            get;
            set;
        }

        public decimal OnlineDeposit
        {
            get;
            set;
        }

        public string OnlinePrecent
        {
            get;
            set;
        }

        public decimal OnlineDepositInMYR
        {
            get;
            set;
        }

        public decimal MobileDeposit
        {
            get;
            set;
        }

        public string MobilePrecent
        {
            get;
            set;
        }

        public decimal MobileDepositInMYR
        {
            get;
            set;
        }

        public decimal LocalDeposit
        {
            get;
            set;
        }

        public string LocalPrecent
        {
            get;
            set;
        }

        public decimal LocalDepositInMYR
        {
            get;
            set;
        }

        public decimal TotalAmount
        {
            get;
            set;
        }
        
        public decimal TotalAmountInMYR
        {
            get;
            set;
        }

        public decimal AverangeDeposit
        {
            get;
            set;
        }

        public int TotalCount
        {
            get;
            set;
        }

        public int SuccessCount
        {
            get;
            set;
        }

        public string SuccessRate
        {
            get;
            set;
        }

        public int FTDCount
        {
            get;
            set;
        }

        public long TotalProcessingTime
        {
            get;
            set;
        }

        public string AverangeProcessingTime
        {
            get;
            set;
        }

    }

    public class DepositByDateReportModel
    {
        public string Date
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string IsoCode
        {
            get;
            set;
        }

        public decimal TotalAmount
        {
            get;
            set;
        }

        public decimal TotalAmountInMYR
        {
            get;
            set;
        }

        public decimal AverangeDeposit
        {
            get;
            set;
        }

        public int TotalCount
        {
            get;
            set;
        }

        public int SuccessCount
        {
            get;
            set;
        }

        public string SuccessRate
        {
            get;
            set;
        }

        public int FTDCount
        {
            get;
            set;
        }
    }

    #endregion

    #region For Get SP Data

    public class DepositReportParticalModel
    {
        public DateTime Date
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string AccountId
        {
            get;
            set;
        }

        public string AccountName
        {
            get;
            set;
        }

        public DepositType DepositType
        {
            get;
            set;
        }

        public int SuccessCount
        {
            get;
            set;
        }

        public int TotalCount
        {
            get;
            set;
        }

        public decimal SuccessRate
        {
            get;
            set;
        }

        public int FTDCount
        {
            get;
            set;
        }

        public decimal DepositSummary
        {
            get;
            set;
        }

        public long TotalProcessingTime
        {
            get;
            set;
        }
    }

    #endregion
}
