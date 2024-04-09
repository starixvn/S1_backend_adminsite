using BackendSite.Service.Model.Common.UploadImage;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL.Interface
{
    public interface IUploadImageBLL
    {
        public Task<UploadResultDto> UploadImageAsync(UploadImageRequest request, UploadImageFunctionEnum uploadImageFunction);

        public Task<UploadResultDto> UploadSlotImageAsync(UploadSlotImageRequest request, UploadImageFunctionEnum uploadImageFunction);
    }
}
