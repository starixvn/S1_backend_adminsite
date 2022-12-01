using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL.Interface;

public interface IAffiliateApiService
{
    Task<AffInfo> QueryAffInfo(string userName, string currency, int site);
}