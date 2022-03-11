using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class TpWallet
    {
        public int TpId { set; get; }
        public string TpName { set; get; }
        public string TpUserName { set; get; }
        public decimal Balance { set; get; }
        public int WalletType { get; set; }
    }
    public class TpInfo
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public string LangId { set; get; }
        public int AppType { set; get; }
        public Dictionary<string, string> inputs { set; get; }
    }
    public class TpTransfer
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public decimal Amount { set; get; }
        public TpDirection direction { set; get; }
        public Dictionary<string, string> inputs { set; get; }
    }
    public class AdminTpTransfer : TpTransfer
    {
        public string Operator { set; get; }
        public string Remarks { set; get; }
    }
    public class TpTransferInfo
    {
        public DateTimeOffset CreateTime { get; set; }
        public string TpName { get; set; }
        public string TransId { get; set; }
        public int Direction { get; set; }
        public decimal Amount { get; set; }
        public string ThirdId { get; set; }
        public string Memo { get; set; }
        public int Status { get; set; }
        public int TpId { get; set; }
        public string DirectionName 
        {
            get
            {
                if(Direction == (int)TpTransferDirectionEnum.ToTP)
                {
                    if(TpId == (int)TpEnum.Saba)
                    {
                        return "Main Wallet -> Sportsbook Wallet";
                    }
                    return $"Main Wallet -> {TpName} Wallet";
                }
                else
                {
                    if (TpId == (int)TpEnum.Saba)
                    {
                        return "Sportsbook Wallet -> Main Wallet";
                    }
                    return $"{TpName} Wallet -> Main Wallet";
                }
            }
        }
    }
    public class AdminTpTransferInfo : TpTransferInfo
    {
        public string SiteName { set; get; }
        public string UserName { set; get; }
        public string Operator { set; get; }
        public string Remarks { set; get; }
    }
    public enum TpDirection
    {
        ToTp = 1,
        ToMain = 2
    }
    public class TransferResult
    {
        public TransferResult()
        {
            Status = TransferStatus.Pending;
        }
        public TransferStatus Status { get; set; }
        public string ThirdId { get; set; }
        public decimal Difference { get; set; }
    }
    public enum TransferStatus
    {
        Success = 0, Fail = 1, Pending = 2, PendingD = 3
    }
    public class UserBalance
    {
        public decimal balance { get; set; }
        public decimal outstanding { get; set; }
        public byte Currency { get; set; }
    }
    public class SetBetLimit
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public string BetSetting { get; set; }
    }
    public class GetBetLimitReq
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
    }
}
