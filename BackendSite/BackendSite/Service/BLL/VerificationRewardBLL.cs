using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class VerificationRewardBLL
    {
        private readonly VerificationRewardService _verificationRewardService;
        private readonly AdjustmentInfoLogService _adjustmentInfoLogService;

        public VerificationRewardBLL(IServiceProvider provider)
        {
            _verificationRewardService = provider.GetService<VerificationRewardService>();
            _adjustmentInfoLogService = provider.GetService<AdjustmentInfoLogService>();
        }
        public async Task<VerificationRewardModel> GetVerificationReward(int siteId, int currencyId)
        {
            return await _verificationRewardService.GetVerificationReward(siteId, currencyId);
        }
        
        public async Task<IEnumerable<VerificationRewardModel>> GetVerificationRewardBySiteId(int siteId)
        {
            return await _verificationRewardService.GetVerificationRewardBySiteId(siteId);
        }
        
        public async Task<int> SetVerificationReward(int siteId, int currencyId, int verificationType, decimal phoneReward, decimal emailReward, string updateOperator)
        {
            var result = await _verificationRewardService.InsertOrUpdate(siteId, currencyId, verificationType, phoneReward, emailReward, updateOperator);
            if (result == 1 || result == 2)
            {
                var para = new
                {
                    siteId,
                    currencyId,
                    verificationType,
                    phoneReward,
                    emailReward
                };
                _adjustmentInfoLogService.InsertAdjustmentInfoLog("VerificationReward", 1, para, updateOperator);
            }

            return result;
        }
        
        public async Task<int> UpdateStatus(int siteId, int currencyId, int status, string updateOperator)
        {
            var result = await _verificationRewardService.UpdateStatus(siteId, currencyId, status, updateOperator);
            if (result == 1)
            {
                var para = new
                {
                    siteId,
                    currencyId,
                    status,
                };
                _adjustmentInfoLogService.InsertAdjustmentInfoLog("VerificationReward", 1, para, updateOperator);
            }
            return result;
        }

        public IEnumerable<CustVerificationRewardModel> GetVerificationRewardReport(List<int> siteIds, int currencyId, string userName,
            DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages,
            out int rowCount)
        {
            var custVerificationReward =  _verificationRewardService.GetCustVerificationReward(siteIds, currencyId, userName, 
                startDate, endDate, pageSize, pageNumber, out totalPages, out rowCount);

            var listRewardModels = new List<CustVerificationRewardModel>();
            foreach (var data in custVerificationReward)
            {
                var model = new CustVerificationRewardModel()
                {
                    SiteName = data.SiteName,
                    CustId = data.CustId,
                    UserName = data.UserName,
                    SabaId = data.SabaId,
                    Agent = data.Agent,
                    CurrencyId = data.CurrencyId,
                    Currency = data.Currency,
                    PhoneVerified = data.PhoneVerified,
                    PhoneReward = data.PhoneReward,
                    PhoneRewardTime = data.PhoneRewardTime,
                    EmailVerified = data.EmailVerified,
                    EmailReward = data.EmailReward,
                    EmailRewardTime = data.EmailRewardTime,
                    VerificationReawardAmount = data.PhoneReward + data.EmailReward,
                    VerificationReawardTime = GetVerificationReawardTime(data.PhoneRewardTime, data.EmailRewardTime)
                };
                listRewardModels.Add(model);
            }

            return listRewardModels;
        }
        
        public async Task<IEnumerable<ExportVerificationRewardModel>> GetVerificationRewardWithoutPage(string siteIds, int currencyId, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int timezone = 8)
        {
            var custVerificationReward = await _verificationRewardService.GetCustVerificationRewardExport(siteIds, currencyId, userName, startDate, endDate);
            var listRewardModels = new List<ExportVerificationRewardModel>();
            foreach (var data in custVerificationReward)
            {
                if (data.PhoneVerified && data.PhoneRewardTime.HasValue && data.PhoneReward != 0 && startDate < data.PhoneRewardTime.Value && endDate > data.PhoneRewardTime)
                {
                    var modelPhone = new ExportVerificationRewardModel()
                    {
                        SiteName = data.SiteName,
                        UserName = data.UserName,
                        Currency = data.Currency,
                        SabaId = data.SabaId,
                        Agent = data.Agent,
                        VerificationType = "Phone Number",
                        RewardAmount = data.PhoneReward,
                        VerificationTime = data.PhoneRewardTime?.ToOffset(TimeSpan.FromHours(timezone)),
                    };
                    listRewardModels.Add(modelPhone); 
                }

                if (data.EmailVerified && data.EmailRewardTime.HasValue && data.EmailReward != 0 && startDate < data.EmailRewardTime.Value && endDate > data.EmailRewardTime)
                {
                    var modelEmail = new ExportVerificationRewardModel()
                    {
                        SiteName = data.SiteName,
                        UserName = data.UserName,
                        Currency = data.Currency,
                        SabaId = data.SabaId,
                        Agent = data.Agent,
                        VerificationType = "Email",
                        RewardAmount = data.EmailReward,
                        VerificationTime = data.EmailRewardTime?.ToOffset(TimeSpan.FromHours(timezone)),
                    };
                    listRewardModels.Add(modelEmail);
                }
               
            }

            return listRewardModels;
        }
        
        public DateTimeOffset GetVerificationReawardTime(DateTimeOffset? phoneRewardTime, DateTimeOffset? emailRewardTime)
        {
            if (phoneRewardTime.HasValue && emailRewardTime.HasValue)
            {
                return phoneRewardTime > emailRewardTime ? phoneRewardTime.Value : emailRewardTime.Value;
            }

            if (phoneRewardTime.HasValue)
                return phoneRewardTime.Value;
            else if (emailRewardTime.HasValue)
                return emailRewardTime.Value;
            else
                return new DateTime(1900, 1, 1);
        }

        public DateTimeOffset? ConvertTimezone(DateTimeOffset? dateTimeOffset, int timezone)
        {
            if (dateTimeOffset.HasValue)
                return dateTimeOffset.Value.ToOffset(TimeSpan.FromHours(timezone));
            return null;
        }
    }
}
