using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface IAffiliateApiService
{
    Task<AffInfo> QueryAffInfo(string userName, string currency, int site);
}