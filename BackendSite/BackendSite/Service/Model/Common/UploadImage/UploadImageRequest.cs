using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Common.UploadImage
{
    public class UploadImageRequest
    {
        public IFormFile File { get; set; }
        /// <summary>
        /// 是否返回完整路徑，完整路徑是會組CloudFrontDomain
        /// </summary>
        public bool ReturnFullPath { get; set; }

        public int SiteId { get; set; }
    }

    public class UploadSlotImageRequest : UploadImageRequest
    {
        public string TpName { get; set; }
        public string Ename { get; set; }
        public string Lang { get; set; }
        public int size { get; set; } 

    }

}
