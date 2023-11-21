using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;
using static BackendSite.Service.BLL.Mpa.MpaWinLossBLL;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL.Mpa.Extension
{
    public static class MpaWinLossExtension
    {
        public static IEnumerable<MpaTpBetRecordDto> Filter(this IEnumerable<MpaTpBetRecordDto> data, MpaWinLossRequest request)
        {

            // 過濾 GameType、CurrencyId、CategoryId
            data = data.Where(r => request.TpGameTypes.Contains(r.GameType) &&
                                   request.CurrencyIds.Contains(r.CurrencyId) && request.TpCategorys.Contains(r.CategoryId));

            // 過濾 UserName
            if (!string.IsNullOrEmpty(request.UserName))
            {
                data = data.Where(r => r.UserName == request.UserName || r.TpUserName == request.UserName);
            }
            //過濾SummaryType
            if (request.DateType == MpaDateType.SettledDate)
            {
                data = data.Where(r => r.SummaryType == 1 || r.SummaryType == 3);
            }
            else if(request.DateType == MpaDateType.CreatedDate)
            {
                data = data.Where(r => r.SummaryType == 2 || r.SummaryType == 4);
            }

            if (!request.NoDraw)
            {
                //Win = 1,Lost = 2,Refund = 3,/*Reject, Refund, Void*/,Draw = 4,Related = 5,FreeBetWin = 6
                data = data.Where(x => x.TicketStatus != 4);
            }

            if (!request.IncludeFreeSpins)
            {
                //Win = 1,Lost = 2,Refund = 3,/*Reject, Refund, Void*/,Draw = 4,Related = 5,FreeBetWin = 6
                data = data.Where(x => x.TicketStatus != 6);
            }


            //For product has jackpot: GGR = - (Player WL – Progressive Win) – Progressive Bet
            foreach (var item in data)
            {
                item.WinLossSummary -= item.ProgressiveWinSummary;
            }

            if (!request.IncludeTest)
            {
                //istest = 0, test member
                data = data.Where(x => x.IsTest == 0);
            }

            return data;
        }

        public static IEnumerable<MpaTpBetRecordDto> SetGgRByOneGamingPoker(this IEnumerable<MpaTpBetRecordDto> data)
        {
            foreach (var item in data)
            {
                if (item.GameType == (int)MpaGGRCustomizationGameTypeEnum.OneGamingPoker)
                {
                    // For p2p products (OneGaming Poker), show commission: GGR = sum(revenue) 
                    item.WinLossSummary = item.RevenueSummary;
                }
            }

            return data;
        }

        public static IEnumerable<MpaTpBetRecordDto> SetGgRByV8Poker(this IEnumerable<MpaTpBetRecordDto> data)
        {
            foreach (var item in data)
            {
                if (item.GameType == (int)MpaGGRCustomizationGameTypeEnum.V8Poker)
                {
                    // For p2p products (V8 Poker), show commission: GGR = sum(revenue) 
                    item.WinLossSummary = item.RevenueSummary;
                }
            }

            return data;
        }
        /// <summary>
        /// 區分出BA資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IEnumerable<MpaTpBetRecordDto> SetBAdata(this IEnumerable<MpaTpBetRecordDto> data)
        {
            foreach (var item in data)
            {
                if (item.GameType == 69 && (item.SummaryType == 3 || item.SummaryType == 4))
                {
                    item.BAStakeSummary = item.StakeSummary;
                    item.BAValidBetSummary = item.ValidBetSummary;
                    item.BARevenueSummary = item.RevenueSummary;
                    item.BAWinLossSummary = item.WinLossSummary;
                    item.BAProgressiveBetSummary = item.ProgressiveBetSummary;
                    item.BAProgressiveWinSummary = item.ProgressiveWinSummary;
                    item.StakeSummary = 0;
                    item.ValidBetSummary = 0;
                    item.RevenueSummary = 0;
                    item.WinLossSummary = 0;
                    item.ProgressiveBetSummary= 0;
                    item.ProgressiveWinSummary= 0;
                }
            }
            return data;
        }

        public static IEnumerable<MpaTpBetRecordDto> SetCommissionByNineW(this IEnumerable<MpaTpBetRecordDto> data,
            MpaWinLossRequest request,
            string format,
            Func<DateTimeOffset, DateTimeOffset, string, MpaDateType, IEnumerable<Commission9WDto>> getCommission)
        {
            var commission = getCommission(request.StarTime, request.EndTime, format, request.DateType);

            // If filtered by Settled Time: Commission = - TpReport9W.commission
            if (request.DateType == MpaDateType.SettledDate)
            {
                foreach (var item in commission)
                {
                    //用commission抓第一筆符合9w/date/custId的資料，並給值
                    var matchRecord = data.FirstOrDefault(r => r.TpId == (int)MpaCommissionCustomizationTpIdEnum.NineW
                                                              && r.Date == item.Date
                                                              && r.CustId == item.CustId);

                    if (matchRecord != null)
                    {
                        matchRecord.RevenueSummary = item.Commission;
                    }
                }
            }
            // If filtered by Created Time: Commission = 0
            else if (request.DateType == MpaDateType.CreatedDate)
            {
                foreach (var d in data)
                {
                    if (d.TpId == (int)MpaCommissionCustomizationTpIdEnum.NineW)
                    {
                        d.RevenueSummary = 0;
                    }
                }
            }

            return data;
        }

        public static DateTime ToOffSetReturnDate(this DateTime dateTime, TimeSpan offset)
        {
            return new DateTimeOffset(dateTime, new TimeSpan(0, 0, 0)).ToOffset(offset).Date;
        }

        public static string DateMapperToRateDate(this DateTime date)
        {
            if (date == DateTime.Today) return date.ToString("yyyy-MM-dd");
            if (date.ToString("yyyy-MM") == DateTime.Now.ToString("yyyy-MM")) return DateTime.UtcNow.ToString("yyyy-MM-dd");
            return date.AddMonths(1).AddDays(-date.AddMonths(1).Day).ToString("yyyy-MM-dd");
        }

        #region ToExcelFormat

        public static IEnumerable<MpaWinLossByDateDtoExport> ToExcelFormat(this List<MpaWinLossByDateDto> originData)
        {
            return originData.GroupBy(x => new { x.Currency, x.Date })
                .Select(item => new MpaWinLossByDateDtoExport
                {
                    Date = item.Key.Date,
                    Currency = item.Key.Currency,
                    TurnOver = item.Sum(x => x.TurnOver).ToString("N2"),
                    ValidTurnOver = item.Sum(x => x.ValidTurnOver).ToString("N2"),
                    GGR = item.Sum(x => x.GGR).ToString("N2"),
                    PlayerWinLoss = item.Sum(x => x.PlayerWinLoss).ToString("N2"),
                    BAPlayerWinLoss = item.Sum(x => x.BAPlayerWinLoss).ToString("N2"),
                    ProgressiveWin = item.Sum(x => x.ProgressiveWin).ToString("N2"),
                    ProgressiveBet = item.Sum(x => x.ProgressiveBet).ToString("N2"),
                    Commission = item.Sum(x => x.Commission).ToString("N2"),
                    BACommission = item.Sum(x => x.BACommission).ToString("N2"),
                    BetCount = item.Sum(x => x.BetCount),
                    AvgBetAmount = item.Sum(x => x.BetCount) != 0
                        ? (item.Sum(x => x.TurnOver) / item.Sum(x => x.BetCount)).ToString("N2")
                        : "0.00m",
                    UniquePlayers = item.Sum(x => x.UniquePlayers),
                    Margin = item.Sum(x => x.ValidTurnOver) != 0
                        ? (item.Sum(x => x.GGR) / item.Sum(x => x.ValidTurnOver)).ToString("N2")
                        : "0.00m",
                    DepositCount = item.Sum(x => x.DepositCount),
                    TotalDepositAmount = item.Sum(x => x.TotalDepositAmount).ToString("N2"),
                    WithDrawCount = item.Sum(x => x.WithDrawCount),
                    TotalWithDrawAmount = item.Sum(x => x.WithDrawCount).ToString("N2"),
                    TotalBonus = item.Sum(x => x.WithDrawCount).ToString("N2"),
                })
                .ToList();
        }

        public static IEnumerable<MpaWinLossByTpGameTypeDtoExport> ToExcelFormat(this List<MpaWinLossByTpGameTypeDto> originData)
        {
            return originData.Select(item => new MpaWinLossByTpGameTypeDtoExport
            {
                TpGameTypeName = item.TpGameTypeName,
                Currency = item.Currency,
                TurnOver = Math.Round(item.TurnOver, 2).ToString("N2"),
                ValidTurnOver = Math.Round(item.ValidTurnOver, 2).ToString("N2"),
                GGR = Math.Round(item.GGR, 2).ToString("N2"),
                PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2).ToString("N2"),
                BAPlayerWinLoss = Math.Round(item.BAPlayerWinLoss, 2).ToString("N2"),
                ProgressiveWin = Math.Round(item.ProgressiveWin, 2).ToString("N2"),
                ProgressiveBet = Math.Round(item.ProgressiveBet, 2).ToString("N2"),
                Commission = Math.Round(item.Commission, 2).ToString("N2"),
                BACommission = Math.Round(item.BACommission, 2).ToString("N2"),
                BetCount = item.BetCount,
                AvgBetAmount = Math.Round(item.AvgBetAmount, 2).ToString("N2"),
                UniquePlayers = item.UniquePlayers,
                Margin = decimal.Round((item.Margin * 100), 2).ToString() + "%"
            })
                .ToList();
        }

        public static IEnumerable<MpaWinLossByTpCategoryDtoExport> ToExcelFormat(this List<MpaWinLossByTpCategoryDto> originData)
        {
            return originData.Select(item => new MpaWinLossByTpCategoryDtoExport
            {
                TpCategoryName = item.TpCategoryName,
                Currency = item.Currency,
                TurnOver = Math.Round(item.TurnOver, 2).ToString("N2"),
                ValidTurnOver = Math.Round(item.ValidTurnOver, 2).ToString("N2"),
                GGR = Math.Round(item.GGR, 2).ToString("N2"),
                PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2).ToString("N2"),
                BAPlayerWinLoss = Math.Round(item.BAPlayerWinLoss, 2).ToString("N2"),
                ProgressiveWin = Math.Round(item.ProgressiveWin, 2).ToString("N2"),
                ProgressiveBet = Math.Round(item.ProgressiveBet, 2).ToString("N2"),
                Commission = Math.Round(item.Commission, 2).ToString("N2"),
                BACommission = Math.Round(item.BACommission, 2).ToString("N2"),
                BetCount = item.BetCount,
                AvgBetAmount = Math.Round(item.AvgBetAmount, 2).ToString("N2"),
                UniquePlayers = item.UniquePlayers,
                Margin = decimal.Round((item.Margin * 100), 2).ToString() + "%"
            })
                .ToList();
        }

        public static IEnumerable<MpaWinLossByPlayerDtoExport> ToExcelFormat(this List<MpaWinLossByPlayerDto> originData)
        {
            return originData.Select(item => new MpaWinLossByPlayerDtoExport
            {
                UserName = item.UserName,
                TpUserName = item.TpUserName,
                Currency = item.Currency,
                GameTypeName = item.GameTypeName,
                TurnOver = Math.Round(item.TurnOver, 2).ToString("N2"),
                ValidTurnOver = Math.Round(item.ValidTurnOver, 2).ToString("N2"),
                GGR = Math.Round(item.GGR, 2).ToString("N2"),
                PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2).ToString("N2"),
                BAPlayerWinLoss = Math.Round(item.BAPlayerWinLoss, 2).ToString("N2"),
                ProgressiveWin = Math.Round(item.ProgressiveWin, 2).ToString("N2"),
                ProgressiveBet = Math.Round(item.ProgressiveBet, 2).ToString("N2"),
                Commission = Math.Round(item.Commission, 2).ToString("N2"),
                BACommission = Math.Round(item.BACommission, 2).ToString("N2"),
                BetCount = item.BetCount,
                AvgBetAmount = Math.Round(item.AvgBetAmount, 2).ToString("N2"),
                UniquePlayers = item.UniquePlayers,
                Margin = decimal.Round((item.Margin * 100), 2).ToString() + "%",
                DepositCount = item.DepositCount,
                TotalDepositAmount = Math.Round(item.TotalDepositAmount, 2).ToString("N2"),
                WithDrawCount = item.WithDrawCount,
                TotalWithDrawAmount = Math.Round(item.TotalWithDrawAmount, 2).ToString("N2"),
                ClosingBalance = Math.Round(item.ClosingBalance, 2).ToString("N2"),
                TotalBonus = Math.Round(item.TotalBonus, 2).ToString("N2")
            })
                .ToList();
        }


        public static IEnumerable<MpaWinLossByCurrencyDtoExport> ToExcelFormat(this List<MpaWinLossByCurrencyDto> originData)
        {
            return originData.Select(item => new MpaWinLossByCurrencyDtoExport
            {
                Currency = item.Currency,
                TurnOver = Math.Round(item.TurnOver, 2).ToString("N2"),
                ValidTurnOver = Math.Round(item.ValidTurnOver, 2).ToString("N2"),
                GGR = Math.Round(item.GGR, 2).ToString("N2"),
                PlayerWinLoss = Math.Round(item.PlayerWinLoss, 2).ToString("N2"),
                BAPlayerWinLoss = Math.Round(item.BAPlayerWinLoss, 2).ToString("N2"),
                ProgressiveWin = Math.Round(item.ProgressiveWin, 2).ToString("N2"),
                ProgressiveBet = Math.Round(item.ProgressiveBet, 2).ToString("N2"),
                Commission = Math.Round(item.Commission, 2).ToString("N2"),
                BACommission = Math.Round(item.BACommission, 2).ToString("N2"),
                BetCount = item.BetCount,
                AvgBetAmount = Math.Round(item.AvgBetAmount, 2).ToString("N2"),
                UniquePlayers = item.UniquePlayers,
                Margin = decimal.Round((item.Margin * 100), 2).ToString() + "%",
                DepositCount = item.DepositCount,
                TotalDepositAmount = Math.Round(item.TotalDepositAmount, 2).ToString("N2"),
                WithDrawCount = item.WithDrawCount,
                TotalWithDrawAmount = Math.Round(item.TotalWithDrawAmount, 2).ToString("N2"),
                TotalBonus = Math.Round(item.TotalBonus, 2).ToString("N2")
            })
                .ToList();
        }

        #endregion

        #region ToApiDataFormat

        public static IEnumerable<MpaWinLossByDateResponseToFrontEnd> ToApiFormat(this Dictionary<string, MpaWinLossByDateResponse> source)
        {
            var result = new List<MpaWinLossByDateResponseToFrontEnd>();

            foreach (var item in source)
            {
                var data = new MpaWinLossByDateResponseToFrontEnd
                {
                    DisplayKey = item.Key,
                    TurnOver = item.Value.TurnOver,
                    ValidTurnOver = item.Value.ValidTurnOver,
                    GGR = item.Value.GGR,
                    BAPlayerWinLoss = item.Value.BAPlayerWinLoss,
                    PlayerWinLoss = item.Value.PlayerWinLoss,
                    ProgressiveWin = item.Value.ProgressiveWin,
                    ProgressiveBet = item.Value.ProgressiveBet,
                    BACommission = item.Value.BACommission,
                    Commission = item.Value.Commission,
                    BetCount = item.Value.BetCount,
                    AvgBetAmount = item.Value.AvgBetAmount,
                    UniquePlayers = item.Value.UniquePlayers,
                    Margin = item.Value.Margin,
                    DepositCount = item.Value.DepositCount,
                    TotalDepositAmount = item.Value.TotalDepositAmount,
                    WithDrawCount = item.Value.WithDrawCount,
                    TotalWithDrawAmount = item.Value.TotalWithDrawAmount,
                    TotalBonus = item.Value.TotalBonus
                };

                // 處理 children
                if (item.Value.CurrencyDetailGroupByCurrency == null)
                {
                    data.Children = new List<MpaWinLossByDayCurrencyDetailDtoToFrontEnd>();
                }
                else
                {
                    var children = item.Value.CurrencyDetailGroupByCurrency
                        .Select(childrenItem => new MpaWinLossByDayCurrencyDetailDtoToFrontEnd { DisplayKey = childrenItem.Currency, Data = childrenItem })
                        .ToList();
                    data.Children = children;
                }

                result.Add(data);
            }

            return result;
        }

        public static IEnumerable<MpaWinLossByTpCategoryResponseToFrontEnd> ToApiFormat(this Dictionary<string, MpaWinLossByTpCategoryResponse> source)
        {
            var result = new List<MpaWinLossByTpCategoryResponseToFrontEnd>();

            foreach (var item in source)
            {
                var data = new MpaWinLossByTpCategoryResponseToFrontEnd
                {
                    DisplayKey = item.Key,
                    TurnOver = item.Value.TurnOver,
                    ValidTurnOver = item.Value.ValidTurnOver,
                    GGR = item.Value.GGR,
                    PlayerWinLoss = item.Value.PlayerWinLoss,
                    BAPlayerWinLoss = item.Value.BAPlayerWinLoss,
                    ProgressiveWin = item.Value.ProgressiveWin,
                    ProgressiveBet = item.Value.ProgressiveBet,
                    Commission = item.Value.Commission,
                    BACommission = item.Value.BACommission,
                    BetCount = item.Value.BetCount,
                    AvgBetAmount = item.Value.AvgBetAmount,
                    UniquePlayers = item.Value.UniquePlayers,
                    Margin = item.Value.Margin
                };

                // 處理 children
                if (item.Value.CurrencyDetailByCategory == null)
                {
                    data.Children = new List<MpaWinLossByTpCategoryCurrencyDetailDtoFrontEnd>();
                }
                else
                {
                    var children = item.Value.CurrencyDetailByCategory
                        .Select(childrenItem => new MpaWinLossByTpCategoryCurrencyDetailDtoFrontEnd() { DisplayKey = childrenItem.Currency, Data = childrenItem })
                        .ToList();
                    data.Children = children;
                }

                result.Add(data);
            }

            return result;
        }

        public static IEnumerable<MpaWinLossByTpGameTypeResponseToFrontEnd> ToApiFormat(this Dictionary<string, MpaWinLossByTpGameTypeResponse> source)
        {
            var result = new List<MpaWinLossByTpGameTypeResponseToFrontEnd>();

            foreach (var item in source)
            {
                var data = new MpaWinLossByTpGameTypeResponseToFrontEnd
                {
                    DisplayKey = item.Key,
                    TurnOver = item.Value.TurnOver,
                    ValidTurnOver = item.Value.ValidTurnOver,
                    GGR = item.Value.GGR,
                    PlayerWinLoss = item.Value.PlayerWinLoss,
                    BAPlayerWinLoss = item.Value.BAPlayerWinLoss,
                    ProgressiveWin = item.Value.ProgressiveWin,
                    ProgressiveBet = item.Value.ProgressiveBet,
                    Commission = item.Value.Commission,
                    BACommission = item.Value.BACommission,
                    BetCount = item.Value.BetCount,
                    AvgBetAmount = item.Value.AvgBetAmount,
                    UniquePlayers = item.Value.UniquePlayers,
                    Margin = item.Value.Margin
                };

                // 處理 children
                if (item.Value.CurrencyDetail == null)
                {
                    data.Children = new List<MpaWinLossByTpGameTypeCurrencyDetailDtoFrontEnd>();
                }
                else
                {
                    var children = item.Value.CurrencyDetail
                        .Select(childrenItem => new MpaWinLossByTpGameTypeCurrencyDetailDtoFrontEnd
                        {
                            DisplayKey = childrenItem.Key,
                            Data = childrenItem.Value
                        })
                        .ToList();

                    data.Children = children;
                }

                result.Add(data);
            }

            return result;
        }

        public static MpaWinLossByPlayerResponseToFrontEndWrapper ToApiFormat(this Dictionary<UserNameKey, MpaWinLossByPlayerResponse> source, int rowCount)
        {
            var response = new MpaWinLossByPlayerResponseToFrontEndWrapper();

            var result = new List<MpaWinLossByPlayerResponseToFrontEnd>();

            foreach (var item in source)
            {
                var data = new MpaWinLossByPlayerResponseToFrontEnd()
                {
                    DisplayKey = item.Key.Total == null ? item.Key.UserName : item.Key.Total,
                    SabaId = item.Value.SabaId,
                    UserName = item.Value.UserName,
                    TurnOver = item.Value.TurnOver,
                    ValidTurnOver = item.Value.ValidTurnOver,
                    GGR = item.Value.GGR,
                    PlayerWinLoss = item.Value.PlayerWinLoss,
                    BAPlayerWinLoss = item.Value.BAPlayerWinLoss,
                    ProgressiveWin = item.Value.ProgressiveWin,
                    ProgressiveBet = item.Value.ProgressiveBet,
                    Commission = item.Value.Commission,
                    BACommission = item.Value.BACommission,
                    BetCount = item.Value.BetCount,
                    AvgBetAmount = item.Value.AvgBetAmount,
                    UniquePlayers = item.Value.UniquePlayers,
                    Margin = item.Value.Margin,
                    ClosingBalance = item.Value.ClosingBalance,
                    DepositCount = item.Value.DepositCount,
                    TotalDepositAmount = item.Value.TotalDepositAmount,
                    WithDrawCount = item.Value.WithDrawCount,
                    TotalWithDrawAmount = item.Value.TotalWithDrawAmount,
                    TotalBonus = item.Value.TotalBonus
                };

                // 處理 children
                if (item.Value.CurrencyDetail == null)
                {
                    data.Children = new List<MpaWinLossByPlayerCurrencyDetailDtoFrontEnd>();
                }
                else
                {
                    var children = item.Value.CurrencyDetail
                        .Select(childrenItem => new MpaWinLossByPlayerCurrencyDetailDtoFrontEnd
                        {
                            DisplayKey = childrenItem.Key,
                            Data = childrenItem.Value
                        })
                        .ToList();
                    data.Children = children;
                }

                result.Add(data);
            }

            response.Data = result;
            response.RowCount = rowCount;

            return response;
        }

        public static IEnumerable<MpaWinLossByPlayerDetailResponseFrontEnd> ToApiFormat(this Dictionary<string, MpaWinLossByPlayerDetailResponse> source)
        {
            var result = new List<MpaWinLossByPlayerDetailResponseFrontEnd>();

            foreach (var item in source)
            {
                var data = new MpaWinLossByPlayerDetailResponseFrontEnd
                {
                    DisplayKey = item.Key,
                    SabaId = item.Value.SabaId,
                    TurnOver = item.Value.TurnOver,
                    ValidTurnOver = item.Value.ValidTurnOver,
                    GGR = item.Value.GGR,
                    PlayerWinLoss = item.Value.PlayerWinLoss,
                    ProgressiveWin = item.Value.ProgressiveWin,
                    ProgressiveBet = item.Value.ProgressiveBet,
                    Commission = item.Value.Commission,
                    BetCount = item.Value.BetCount,
                    AvgBetAmount = item.Value.AvgBetAmount,
                    UniquePlayers = item.Value.UniquePlayers,
                    Margin = item.Value.Margin
                };
                result.Add(data);
            }

            return result;
        }

        public static IEnumerable<MpaWinLossByCurrencyResponseFrontEnd> ToApiFormat(this Dictionary<string, MpaWinLossByCurrencyResponse> source)
        {
            var result = new List<MpaWinLossByCurrencyResponseFrontEnd>();

            foreach (var item in source)
            {
                var data = new MpaWinLossByCurrencyResponseFrontEnd()
                {
                    DisplayKey = item.Key,
                    TurnOver = item.Value.TurnOver,
                    ValidTurnOver = item.Value.ValidTurnOver,
                    GGR = item.Value.GGR,
                    PlayerWinLoss = item.Value.PlayerWinLoss,
                    BAPlayerWinLoss = item.Value.BAPlayerWinLoss,
                    ProgressiveWin = item.Value.ProgressiveWin,
                    ProgressiveBet = item.Value.ProgressiveBet,
                    Commission = item.Value.Commission,
                    BACommission = item.Value.BACommission,
                    BetCount = item.Value.BetCount,
                    AvgBetAmount = item.Value.AvgBetAmount,
                    UniquePlayers = item.Value.UniquePlayers,
                    Margin = item.Value.Margin,
                    DepositCount = item.Value.DepositCount,
                    TotalDepositAmount = item.Value.TotalDepositAmount,
                    WithDrawCount = item.Value.WithDrawCount,
                    TotalWithDrawAmount = item.Value.TotalWithDrawAmount,
                    TotalBonus = item.Value.TotalBonus
                };

                // 處理 children
                if (item.Value.CurrencyDetail == null)
                {
                    data.Children = new List<MpaWinLossByCurrencyCurrencyDetailDtoFrontEnd>();
                }
                else
                {
                    var children = item.Value.CurrencyDetail
                        .Select(childrenItem => new MpaWinLossByCurrencyCurrencyDetailDtoFrontEnd
                        {
                            DisplayKey = childrenItem.Key,
                            Data = childrenItem.Value
                        })
                        .ToList();

                    data.Children = children;
                }

                result.Add(data);
            }

            return result;
        }

        #endregion
    }
}


