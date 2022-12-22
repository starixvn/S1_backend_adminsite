namespace BackendSite.Service.Model.GameStatisticalReport
{
    public class GameStatisticalByCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<GameStatisticalDao> GameDetail { get; set; }
    }
}
