namespace BackendSite.Service.Model.Report;

public class MultilevelReport<T>
{
    public string Key { get; set; }
    public IEnumerable<T> Children { get; set; }
}