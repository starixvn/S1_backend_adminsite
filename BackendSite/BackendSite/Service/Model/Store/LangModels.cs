namespace BackendSite.Service.Model.Store
{
    public class LangNameModel
    {
        public string Lang { get; set; }
        public string Name { get; set; }
    }


    public class LangInfoModel : LangNameModel
    {
        public string Description { get; set; }
    }
}