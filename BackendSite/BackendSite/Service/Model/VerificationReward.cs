using System;
using System.Collections.Generic;
using BackendSite.Service.Model.ApiModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace BackendSite.Service.Model
{
    public class GetVerificationRewardRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }

    public class VerificationRewardModel
    {
        private DateTimeOffset _createTime;
        private DateTimeOffset? _lastUpdateTime;
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int VerificationType { get; set; }
        public decimal PhoneReward { get; set; }
        public decimal EmailReward { get; set; }
        public int Status { get; set; }

        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string LastUpdateUser { get; set; }

        public DateTimeOffset? LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                if (value.HasValue)
                    _lastUpdateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
    }

    public class UpdateVerificationRewardStatusReq
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
    }

    public class SetVerificationRewardRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int VerificationType { get; set; }
        public decimal PhoneReward { get; set; }
        public decimal EmailReward { get; set; }
    }

    public class QueryVerificationRewardReport : ApiRequestPageModel
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public List<int> SiteIds { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }

        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }

        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    
    public class CustVerificationReward
    {
        private DateTimeOffset? _phoneRewardTime;
        private DateTimeOffset? _emailRewardTime;
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public bool PhoneVerified { get; set; }
        public decimal PhoneReward { get; set; }

        public DateTimeOffset? PhoneRewardTime
        {
            get => _phoneRewardTime;
            set
            {
                if (value.HasValue)
                    _phoneRewardTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }

        public bool EmailVerified { get; set; }
        public decimal EmailReward { get; set; }

        public DateTimeOffset? EmailRewardTime
        {
            get => _emailRewardTime;
            set
            {
                if (value.HasValue)
                    _emailRewardTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
    }
    
    public class CustVerificationRewardModel
    {
        private DateTimeOffset? _phoneRewardTime;
        private DateTimeOffset? _emailRewardTime;
        private DateTimeOffset _verificationReawardTime;
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public bool PhoneVerified { get; set; }
        public decimal PhoneReward { get; set; }

        public DateTimeOffset? PhoneRewardTime
        {
            get => _phoneRewardTime;
            set
            {
                if (value.HasValue)
                    _phoneRewardTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }

        public bool EmailVerified { get; set; }
        public decimal EmailReward { get; set; }

        public DateTimeOffset? EmailRewardTime
        {
            get => _emailRewardTime;
            set
            {
                if (value.HasValue)
                    _emailRewardTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }

        public decimal VerificationReawardAmount { get; set; }

        public DateTimeOffset VerificationReawardTime
        {
            get => _verificationReawardTime;
            set => _verificationReawardTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    public class ExportVerificationRewardModel
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public bool PhoneVerified { get; set; }
        public decimal PhoneReward { get; set; }
        public DateTimeOffset? PhoneRewardTime { get; set; }
        public bool EmailVerified { get; set; }
        public decimal EmailReward { get; set; }
        public DateTimeOffset? EmailRewardTime { get; set; }
        public decimal VerificationReawardAmount { get; set; }
        public DateTimeOffset VerificationReawardTime { get; set; }
    }
}