namespace BackendSite.Service.Model.ApiModel
{
    public interface IOrderBy
    {
        bool OrderBy { get; set; }
        string OrderByProperty { get; set; }
        OrderByDirection OrderByDirection { get; set; }
    }

    public enum OrderByDirection
    {
        Ascending = 0,
        Descending = 1
    }
}