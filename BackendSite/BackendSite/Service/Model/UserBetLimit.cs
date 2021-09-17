namespace BackendSite.Service.Model
{
    public class UserBetLimit
    {
        
    }

    public class TpCategoryData
    {
        public int CategoryId { set; get; }
        public string CategoryName { set; get; }
    }

    public class TpCategoryBetLimit
    {
        public int CategoryId { set; get; }
        public string CategoryName { set; get; }
        public bool Checked { get; set; }
        public int MinBet { set; get; }
        public int MaxBet { set; get; }
    }
}