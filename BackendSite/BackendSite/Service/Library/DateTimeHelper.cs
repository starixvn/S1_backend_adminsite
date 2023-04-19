using BackendSite.Service.DAL;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Library;

public class DateTimeHelper
{
    private readonly ReportService _reportService;
    private readonly ICacheHelper _redisHelper;

    public DateTimeHelper(IServiceProvider provider)
    {
        _reportService = provider.GetRequiredService<ReportService>();
        _redisHelper = provider.GetRequiredService<ICacheHelper>();
    }

    public DateTimeOffset ToCurrencyZone(DateTime time, int currencyId) 
    {
        var timezone = _redisHelper.TryGetOrCreate($"TimeZone:{currencyId}", null,
            () => _reportService.GetCurrencyList()
                .FirstOrDefault(x => x.CurrencyId == currencyId)
                ?.TimeZone, RedisDataBase.Shared);

        return new DateTimeOffset(new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second), TimeSpan.Zero)
            .ToOffset(TimeSpan.Parse(timezone.TrimStart('+')));
    }
}