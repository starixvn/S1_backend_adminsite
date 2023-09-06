namespace BackendSite.Service.Model.GameStatisticalReport
{
    public class GameStatisticalByCurrencyDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public IEnumerable<GameStatisticalByCategoryDto> CategoryDetail { get; set; }
    }
}
