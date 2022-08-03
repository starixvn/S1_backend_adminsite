using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.BLL
{
    public class WorldCupPredictionBLL
    {
        private readonly IWorldCupPredictionService _worldCupPredictionService;
        private readonly ILogger<WorldCupPredictionBLL> _logger;
        public WorldCupPredictionBLL(IWorldCupPredictionService worldCupPredictionService, ILogger<WorldCupPredictionBLL> logger)
        {
            this._worldCupPredictionService = worldCupPredictionService;
            _logger = logger;
        }

        public WorldCupPredictionSettingResponse GetCupPredictionSettingResponse(string langId)
        {
            var dbData = _worldCupPredictionService.GetWorldCupPredictionSetting(langId).ToList();
            var options = GetWorldCupPredictionSelectOptions();

            var responseData = new WorldCupPredictionSettingResponse()
            {
                SelectOptions = options,
                Top32Team = FilterData(dbData, "top32Team"),
                RoundOf16 = FilterData(dbData, "roundOf16"),
                QuarterFinals = FilterData(dbData, "quarterFinals"),
                SemiFinals = FilterData(dbData, "semiFinals"),
                ThirdRdPlace = FilterData(dbData, "3rdPlace"),
                Final = FilterData(dbData, "final"),
            };

            return responseData;
        }

        public async Task SetWorldCupPredictionSetting(SetWorldCupPredictionSettingRequest request)
        {
            var updateData = MapToWorldCupPredictionSettingDto(request).ToList();
            if (updateData.Any())
            {
                try
                {
                    await _worldCupPredictionService.BeginTransaction();
                    await _worldCupPredictionService.SetWorldCupPrediction(); // 先將原本啟用的資料全部更新未啟用
                    var tasks = updateData.Select(item => _worldCupPredictionService.InsertWorldCupPrediction(item)).ToList(); // Insert
                    await Task.WhenAll(tasks);
                    await _worldCupPredictionService.Commit();
                }
                catch (Exception ex)
                {
                    await _worldCupPredictionService.RollBack();
                    _logger.LogError(ex.Message, ex);
                }
            }

            await Task.CompletedTask;
        }

        private IEnumerable<WorldCupPredictionSelectOptions> GetWorldCupPredictionSelectOptions()
        {
            var dbData = _worldCupPredictionService.GetWorldCupPredictionOptions();

            var result = dbData.Select(r => new WorldCupPredictionSelectOptions
            {
                CountryId = r.TeamId,
                CountryName = r.TeamName
            });

            return result;
        }

        private IEnumerable<WorldCupEventSetting> FilterData(IEnumerable<WorldCupPredictionSettingDto> sourceData, string levelName)
        {
            return sourceData.Where(r => r.LevelName == levelName)
                .OrderBy(c => c.Sort)
                .Select(x => new WorldCupEventSetting
                {
                    CountryId = x.TeamId,
                    CountryName = x.TeamName,
                    Sort = x.Sort,
                }).ToList();
        }

        private IEnumerable<WorldCupPredictionSettingDto> MapToWorldCupPredictionSettingDto(SetWorldCupPredictionSettingRequest request)
        {
            var updateData = new List<WorldCupPredictionSettingDto>();

            if (request.Data.Top32Team.Any()) // 32強
                updateData.AddRange(request.Data.Top32Team.Select(r => new WorldCupPredictionSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "top32Team",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.RoundOf16.Any()) // 16強
                updateData.AddRange(request.Data.RoundOf16.Select(r => new WorldCupPredictionSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "roundOf16",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.QuarterFinals.Any())  // 8強
                updateData.AddRange(request.Data.QuarterFinals.Select(r => new WorldCupPredictionSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "quarterFinals",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.SemiFinals.Any()) // 4強
                updateData.AddRange(request.Data.SemiFinals.Select(r => new WorldCupPredictionSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "semiFinals",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.ThirdRdPlace.Any()) // 季軍
                updateData.AddRange(request.Data.ThirdRdPlace.Select(r => new WorldCupPredictionSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "3rdPlace",
                    Sort = r.Sort
                }).ToList());

            if (request.Data.Final.Any())  // 冠亞
                updateData.AddRange(request.Data.Final.Select(r => new WorldCupPredictionSettingDto()
                {
                    TeamId = r.CountryId,
                    LevelName = "final",
                    Sort = r.Sort
                }).ToList());

            return updateData;
        }

        public IEnumerable<GetWorldCupMemberReportResponse> GetMemberReport(GetWorldCupMemberReportRequest request, out int totalPages, out int rowCount)
        {
            var memberReport = _worldCupPredictionService.GetMemberReport(request, out totalPages, out rowCount);
            return memberReport.Select(a => new GetWorldCupMemberReportResponse()
            {
                Currency = a.Currency,
                UserName = a.UserName,
                TicketType = a.TicketType,
                TeamName = a.TeamName,
                SelectTime = DatetimeFormat(a.SelectTime)
            });
        }

        public async Task<IEnumerable<ExportWorldCupMemberReportResponse>> GetMemberReport(ExportWorldCupMemberReportRequest request)
        {
            var memberReport = await _worldCupPredictionService.GetMemberReport(request);
            return memberReport.Select(a => new ExportWorldCupMemberReportResponse()
            {
                Currency = a.Currency,
                UserName = a.UserName,
                TicketType = $"Ticket {a.TicketType}",
                TeamName = a.TeamName,
                SelectTime = a.SelectTime?.AddHours(request.Timezone)
            });
        }

        public async Task<IEnumerable<GetWorldCupEventReportResponse>> GetPredictiveStatistics(int siteId)
        {
            var eventReport = await _worldCupPredictionService.GetPredictiveStatistics(siteId);
            return eventReport.Select(a => new GetWorldCupEventReportResponse()
            {
                TeamId = a.TeamId,
                TeamName = a.TeamName,
                PredictUserCount = a.PredictUserCount,
                IsPayoutDone = a.UpdateTime.HasValue,
                Operator = a.Operator,
                UpdateTime = DatetimeFormat(a.UpdateTime)
            });
        }

        public async Task<IEnumerable<ExportWorldCupEventReportResponse>> GetPredictDetail(int siteId, int teamId)
        {
            var predictDetail = await _worldCupPredictionService.GetPredictDetail(siteId, teamId);
            return predictDetail.Select(a => new ExportWorldCupEventReportResponse()
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
            return await _worldCupPredictionService.SetPayoutDone(siteId, teamId, updateOperator);
        }

        private string DatetimeFormat(DateTime? dateTime)
        {
            return dateTime.HasValue ? $"{dateTime.Value:yyyy-MM-ddTHH:mm:ss}+00:00" : null;
        }
    }
}
