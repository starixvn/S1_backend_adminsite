using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface ICustPhotoIdBLL
{
    Task<IEnumerable<GetCustPhotoIdResponse>> GetPhotoIdAsync(int custId);
    Task<int> UploadPhotoIdAsync(int custId, IFormFile photo);
    Task<int> DeleteCustPhotoId(int custId, int id);
}