using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{


    public class CryptoWithdrawalLimitInfoManage : USDTWithdrawalLimitInfoManage
    {
        public string LevelName { get; set; }
        public int Times { get; set; }
        public int CryptoCurrencyId { get; set; }
    }

    public class CryptoInfo
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string Network { get; set; }
        public int NetworkType
            => (int)(NetworkTypeEnum)Enum.Parse(typeof(NetworkTypeEnum), this.Network);

        /// <summary>
        /// 1: Enable, 0: Disable
        /// </summary>
        public int Status { get; set; }
    }

    public class CryptoInfoData
    {
        /// <summary>
        /// 加密貨幣幣別
        /// </summary>
        public IEnumerable<CryptoCurrency> CryptoCurrency { get; set; }

        /// <summary>
        /// 加密貨幣協定
        /// </summary>
        public IEnumerable<CryptoNetwork> CryptoNetwork { get; set; }
    }

    public class CryptoCurrency
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
    }

    internal class CryptoCurrencyComparer : EqualityComparer<CryptoCurrency>
    {
        public override bool Equals(CryptoCurrency x, CryptoCurrency y)
        {
            return x.CurrencyId == y.CurrencyId
                && x.CurrencyName == y.CurrencyName;
        }

        public override int GetHashCode(CryptoCurrency obj)
        {
            return 0;
        }
    }

    public class CryptoNetwork
    {
        public int CurrencyId { get; set; }
        public string Network { get; set; }
        public int NetworkType;
    }
}
