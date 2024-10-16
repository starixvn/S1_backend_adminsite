using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;

namespace BackendSite.Service.BLL;

public class AWSS3UseBLL
{
    #region Fields


    private readonly AWSS3Helper _awsS3Helper;
    private readonly AWSS3WithMFAHelper _awsS3WithMFAHelper;


    #endregion

    #region Constructors

    public AWSS3UseBLL(IServiceProvider provider)
    {
        _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
        _awsS3WithMFAHelper = provider.GetRequiredService<AWSS3WithMFAHelper>();
    }

    #endregion
}
