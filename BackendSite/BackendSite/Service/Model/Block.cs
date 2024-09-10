using BackendSite.Attributes;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class GetBlockIPListRequest : ApiRequestPageModel
    {
        public string IP { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
    public class BlockIPPageData
    {
        public IEnumerable<BlockIP> BlockIP { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

    public class BlockIP
    {
        public string IP { get; set; }
        public string Memo { get; set; }
        public string Operator { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Location { get; set; }
    }

    public class BlockIPForUpload
    {
        public string IP
        {
            get => $"{_IP} 1;";
            set => _IP = value;
        }

        private string _IP { get; set; }
    }

    public class BlockBankAcc
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNum { get; set; }
        public int SysId { get; set; }
        public string IFSCCode { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset? _blockUpdateTime { get; set; }
        public DateTimeOffset? BlockUpdateTime
        {
            get => _blockUpdateTime;
            set => _blockUpdateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
        public int Status { get; set; }
        public int BlockStatus { get; set; }
        public int CurrencyId { get; set; }
    }
    public class BlockList
    {
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string AccountName { get; set; }
        public string IFSCCode { get; set; }
    }

    public class IPUser
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
    }
    public class DeviceUser
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
    }

    public class AssociatedDevice
    {
        public string DeviceId { get; set; }
    }
    public class BlockAccountSetRequest
    {
        public int SysId { get; set; }
        public string AccountNum { get; set; }
        [LogIgnore]
        public BlockAccountStatus Status { get; set; }
        public string StatusName => Status.ToString();
        [LogIgnore]
        public string Memo { get; set; }
    }
    public class ReleaseAccountRequest
    {
        public int SysId { get; set; }
        public string AccountNum { get; set; }
        public int CustId { get; set; }
        public string IFSCCode { get; set; }
    }
    public class BlockIPRequest
    {
        public String IP { get; set; }
    }
    public class GetDeviceUserListRequest
    {
        public int CustId { get; set; }
        public string DeviceId { get; set; }
    }
    public class SetBlockIPRequest : BlockIPRequest
    {
        public string Memo { get; set; }
    }

    public class BlockCryptoWallet
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string AccountName { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset? _blockUpdateTime { get; set; }
        public DateTimeOffset? BlockUpdateTime
        {
            get => _blockUpdateTime;
            set => _blockUpdateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
        public int Status { get; set; }
        public int BlockStatus { get; set; }
        public int CurrencyId { get; set; }
        public string Network { get; set; }
        public string Address { get; set; }
        public string CryptoType { get; set; }
    }
    public class BlockCryptoWalletList
    {
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string AccountName { get; set; }
        public string Address { get; set; }
        public string Network { get; set; }
        public string NickName { get; set; }
        public int CurrencyId { get; set; }
    }

    public class BlockCryptoWalletSetRequest
    {
        public string Address { get; set; }
        [LogIgnore]
        public BlockCryptoWalletStatus Status { get; set; }
        public string StatusName => Status.ToString();
        [LogIgnore]
        public string Memo { get; set; }
    }
    public class ReleaseCryptoWalletRequest
    {
        public string Address { get; set; }
        public int CustId { get; set; }
        public string Network { get; set; }
        public int CurrencyId { get; set; }
    }
    public class CustCryptoWalletList
    {
        public string AccountName { get; set; }
        public string Address { get; set; }
        public string Network { get; set; }
        public string NickName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset? _blockUpdateTime { get; set; }
        public DateTimeOffset? BlockUpdateTime
        {
            get => _blockUpdateTime;
            set => _blockUpdateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
        public int Status { get; set; }
        public int BlockStatus { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
}
