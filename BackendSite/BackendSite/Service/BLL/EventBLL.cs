using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class EventBLL
    {
        private readonly IEventService _eventService;
        private readonly MpaCommonService _mpaCommonService;
        private readonly ILogger<EventBLL> _logger;
        public EventBLL(IServiceProvider provider)
        {
            _eventService = provider.GetRequiredService<IEventService>();
            _mpaCommonService = provider.GetRequiredService<MpaCommonService>();
            _logger = provider.GetRequiredService<ILogger<EventBLL>>();
        }

        #region CupEvent
        
        public CupEventPredictionSettingResponse GetCupPredictionSettingResponse(string langId)
        {
            var dbData = _eventService.GetCupEventSetting(langId).ToList();
            var options = GetCupEventPredictionSelectOptions(langId);

            var responseData = new CupEventPredictionSettingResponse()
            {
                SelectOptions = options,
                Top24Team = FilterData(dbData, "top24Team"),
                RoundOf16 = FilterData(dbData, "roundOf16"),
                QuarterFinals = FilterData(dbData, "quarterFinals"),
                SemiFinals = FilterData(dbData, "semiFinals"),
                ThirdRdPlace = FilterData(dbData, "3rdPlace"),
                Final = FilterData(dbData, "final"),
            };

            return responseData;
        }

        public async Task SetCupEventPredictionSetting(SetCupEventPredictionSettingRequest request)
        {
            var updateData = MapToCupEventPredictionSettingDto(request).ToList();
            if (updateData.Any())
            {
                try
                {
                    await _eventService.BeginTransaction();
                    await _eventService.SetCupEventPrediction(); // 先將原本啟用的資料全部更新未啟用
                    var tasks = updateData.Select(item => _eventService.InsertCupEventPrediction(item)).ToList(); // Insert
                    await Task.WhenAll(tasks);
                    await _eventService.Commit();
                }
                catch (Exception ex)
                {
                    await _eventService.RollBack();
                    _logger.LogError(ex.Message, ex);
                }
            }

            await Task.CompletedTask;
        }

        private IEnumerable<CupEventPredictionSelectOptions> GetCupEventPredictionSelectOptions(string landId)
        {
            var dbData = _eventService.GetCupEventOptions(landId);

            var result = dbData.Select(r => new CupEventPredictionSelectOptions
            {
                CountryId = r.TeamId,
                CountryName = r.TeamName
            });

            return result;
        }

        private IEnumerable<CupEventSetting> FilterData(IEnumerable<CupEventSettingDto> sourceData, string levelName)
        {
            return sourceData.Where(r => r.LevelName == levelName)
                .OrderBy(c => c.Sort)
                .Select(x => new CupEventSetting
                {
                    CountryId = x.TeamId,
                    CountryName = x.TeamName,
                    Sort = x.Sort,
                }).ToList();
        }

        private IEnumerable<CupEventSettingDto> MapToCupEventPredictionSettingDto(SetCupEventPredictionSettingRequest request)
        {
            var updateData = new List<CupEventSettingDto>();

            if (request.Data.Top24Team.Any()) // 24強
                updateData.AddRange(request.Data.Top24Team.Select(r => new CupEventSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "top24Team",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.RoundOf16.Any()) // 16強
                updateData.AddRange(request.Data.RoundOf16.Select(r => new CupEventSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "roundOf16",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.QuarterFinals.Any())  // 8強
                updateData.AddRange(request.Data.QuarterFinals.Select(r => new CupEventSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "quarterFinals",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.SemiFinals.Any()) // 4強
                updateData.AddRange(request.Data.SemiFinals.Select(r => new CupEventSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "semiFinals",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.ThirdRdPlace.Any()) // 季軍
                updateData.AddRange(request.Data.ThirdRdPlace.Select(r => new CupEventSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "3rdPlace",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.Final.Any())  // 冠亞
                updateData.AddRange(request.Data.Final.Select(r => new CupEventSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "final",
                    Sort = r.Sort
                }).ToList());

            return updateData;
        }

        public IEnumerable<GetCupEventMemberReportResponse> GetMemberReport(GetCupEventMemberReportRequest request, out int totalPages, out int rowCount)
        {
            var memberReport = _eventService.GetMemberReport(request, out totalPages, out rowCount);
            return memberReport.Select(a => new GetCupEventMemberReportResponse()
            {
                Currency = a.Currency,
                UserName = a.UserName,
                TicketType = a.TicketType,
                TeamName = a.TeamName,
                SelectTime = DatetimeFormat(a.SelectTime)
            });
        }

        public async Task<IEnumerable<ExportCupEventMemberReportResponse>> GetMemberReport(ExportCupEventMemberReportRequest request)
        {
            var memberReport = await _eventService.GetMemberReport(request);
            return memberReport.Select(a => new ExportCupEventMemberReportResponse()
            {
                Currency = a.Currency,
                UserName = a.UserName,
                TicketType = $"Ticket {a.TicketType}",
                TeamName = a.TeamName,
                SelectTime = a.SelectTime
            });
        }

        public async Task<IEnumerable<GetCupEventReportResponse>> GetPredictiveStatistics(int siteId)
        {
            var eventReport = await _eventService.GetPredictiveStatistics(siteId);
            return eventReport.Select(a => new GetCupEventReportResponse()
            {
                TeamId = a.TeamId,
                TeamName = a.TeamName,
                PredictUserCount = a.PredictUserCount,
                IsPayoutDone = a.UpdateTime.HasValue,
                Operator = a.Operator,
                UpdateTime = DatetimeFormat(a.UpdateTime)
            });
        }

        public async Task<IEnumerable<ExportCupEventReportResponse>> GetPredictDetail(int siteId, int teamId)
        {
            var predictDetail = await _eventService.GetPredictDetail(siteId, teamId);
            return predictDetail.Select(a => new ExportCupEventReportResponse()
            {
                Currency = a.Currency,
                UserName = a.UserName,
                TeamName = a.TeamName,
                TicketType = $"Ticket {a.TicketType}",
                SelectTime = new DateTimeOffset(a.SelectTime, TimeSpan.Zero)
            });
        }

        public async Task<int> SetPayoutDone(int siteId, int teamId, string updateOperator)
        {
            return await _eventService.SetPayoutDone(siteId, teamId, updateOperator);
        }

        private string DatetimeFormat(DateTime? dateTime)
        {
            return dateTime.HasValue ? $"{dateTime.Value:yyyy-MM-ddTHH:mm:ss}+00:00" : null;
        }
        public async Task<IEnumerable<ExportCupEventPredictScoreResponse>> GetCupEventPredictScore()
        {
            var predictDetail = await _eventService.GetCupEventPredictScore();
            return predictDetail.Select(a => new ExportCupEventPredictScoreResponse
            {
                SiteName = a.SiteName,
                Currency = a.Currency,
                CustId = a.CustId,
                UserName = a.UserName,
                HomeScore = a.HomeScore,
                AwayScore = a.AwayScore,
                CreateTime = new DateTimeOffset(a.CreateTime, TimeSpan.Zero)
            });
        }

        #endregion
        
        #region Lottery

        public IEnumerable<LotteryEventResponse> GetLotteryEventSetting(GetLotteryEventRequest request)
        {
            var responses = _eventService.GetLotteryEventSetting(request);
           
            return responses;
        }

        public LotteryEventResponse GetLotteryEventSettingBySq(int sq)
        {
            var responses = _eventService.GetLotteryEventSettingBySq(sq);
           
            return responses;
        }
        
        public async Task<int> SetLotteryEventSetting(SetLotteryEventRequest request , string updator)
        {
            var setting = new List<int>
            {
                request.FirstPrize, 
                request.SecondPrize, 
                request.ThirdPrize, 
                request.Consolation, 
                request.Small
            };
            var excludeSetting = JsonConvert.SerializeObject(setting);
            var result = await _eventService.SetLotteryEventSetting(request.Sq, request.StartDate, request.EndDate ,request.DrawDate ,request.IsSuspend , excludeSetting ,  JsonConvert.SerializeObject(request.ExcludeTagList),updator);
            
            return result;
        }
        
        public async Task<int> AddLotteryEventSetting(AddLotteryEventRequest request , string updator)
        {
            var setting = new List<int>
            {
                request.FirstPrize, 
                request.SecondPrize, 
                request.ThirdPrize, 
                request.Consolation, 
                request.Small
            };
            var excludeSetting = JsonConvert.SerializeObject(setting);
            var result = await _eventService.AddLotteryEvent(request.SiteId, request.StartDate, request.EndDate ,request.DrawDate , excludeSetting , JsonConvert.SerializeObject(request.ExcludeTagList),updator);
            
            return result;
        }
        
        public LotteryTicketHistory GetLotteryTicketHistory(int siteId, int roundId)
        {
            var responses = _eventService.GetLotteryTicketHistory(siteId, roundId);
           
            return responses;
        }
        
        public IEnumerable<GetLotteryEventRateResponse> GetLotteryRate(int sq)
        {
            var data = _eventService.GetLotteryEventSettingBySq(sq);
            var rateDate = new DateTimeOffset(new DateTime(data.StartDate.Year, data.StartDate.Month, 1), new TimeSpan(8,0,0));
            var rateList = _mpaCommonService.GetRate(rateDate.DateTime, rateDate.DateTime, 2).Result.ToList();
            
            var responses = _eventService.GetRaffleTicketStickerSetting(data.SiteId)
                .Select(x => new GetLotteryEventRateResponse 
                {
                    RateDate = rateDate.DateTime,
                    TargetCurrencyId = x.CurrencyId,
                    IsoCode = x.IsoCode
                }).ToList();
            
            foreach (var responseData in responses)
            {
                var rateDto = rateList.FirstOrDefault(x => x.CurrencyId == 3 && x.TargetCurrencyId == responseData.TargetCurrencyId);

                if (rateDto is null)
                {
                    rateDto = rateList.FirstOrDefault(x => x.CurrencyId == responseData.TargetCurrencyId && x.TargetCurrencyId == 3);
                    if (rateDto is null)
                    {
                        continue;
                    }
                    responseData.Rate = rateDto.Rate;
                    responseData.RateFunction = -1;
                }
                else
                {
                    responseData.Rate = rateDto.Rate;
                }
            }
            
            return responses;
        }
        
        public IEnumerable<RaffleTicketStickerResponse> GetRaffleTicketStickerSetting(GetRaffleTicketStickerRequest request)
        {
            var responses = _eventService.GetRaffleTicketStickerSetting(request.SiteId);
           
            return responses;
        }
        
        public async Task<int> SetRaffleTicketStickerSetting(SetRaffleTicketStickerRequest request , string updator)
        {
            var result = await _eventService.SetRaffleTicketStickerSetting(request ,updator);
            
            return result;
        }
        
        public async Task<GetStickerCountResponse> GetStickerCount(GetStickerCountRequest request )
        {
            var result = await _eventService.GetStickerCount(request);
            
            return result;
        }
        
        public async Task<int> SetStickerCount(SetStickerCountRequest request )
        {
            var result = await _eventService.SetStickerCount(request);
            
            return result;
        }
        
        public IEnumerable<EventRestrictionResponse> GetEventRestrictionList(GetEventRestrictionRequest request)
        {
            var responses = _eventService.GetEventRestrictionList(request);
           
            return responses;
        }
        
        public async Task<int> InsEventRestriction(int siteId, int currencyId, int custId ,string reason ,string creator)
        {
            var result = await _eventService.InsEventRestriction(siteId ,currencyId ,custId , reason ,creator);
            
            return result;
        }
        
        public async Task<int> DelEventRestriction(int custId)
        {
            var result = await _eventService.DelEventRestriction(custId);
            
            return result;
        }

        public async Task<GetStickerReportResponse> GetStickerReport(GetStickerReportRequest request)
        {
            var currencyTotalAmount = new List<StickerReportCurrencyTotal>();

            if (string.IsNullOrEmpty(request.RoundId))
            {
                request.RoundId = "0";
            }
            
            var result = await _eventService.GetStickerReport(request);
            
            var totalAmounts = result.StickerReportData
                .GroupBy(p => new {p.CurrencyId , p.IsoCode} )
                .Select(g => new StickerReportCurrencyTotal
                {
                    CurrencyId = g.Key.CurrencyId,
                    IsoCode = g.Key.IsoCode,
                    CurrencyCount = g.Count(),
                    CurrencyAmount = g.Sum(p => p.StickerAmount)
                })
                .ToList();

            currencyTotalAmount.AddRange(totalAmounts);
            result.StickerReportCurrencyTotal = currencyTotalAmount;
            
            return result;
        }

        public async Task<IEnumerable<GetStickerReportDto>> GetStickerReportExport(GetStickerReportExportRequest request)
        {
            if (string.IsNullOrEmpty(request.RoundId))
            {
                request.RoundId = "0";
            }
            
            //先拉10000筆
            var gerRequest = new GetStickerReportRequest
            {
                PageNumber = 1,
                PageSize = 10000,
                SiteIds = request.SiteIds,
                UserName = request.UserName,
                RoundId = request.RoundId,
                CurrencyIds = request.CurrencyIds,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            
            var result = await _eventService.GetStickerReport(gerRequest);
            return result.StickerReportData;
        }

        public IEnumerable<GetCustLotteryTicket> GetCustLotteryTicket(int siteId , string currencyIds, string userName , int roundId ,string ticketNo)
        {
            var responses = _eventService.GetCustLotteryTicket(siteId, currencyIds, userName, roundId, ticketNo);
           
            return responses;
        }
        
        #endregion
    }
}
