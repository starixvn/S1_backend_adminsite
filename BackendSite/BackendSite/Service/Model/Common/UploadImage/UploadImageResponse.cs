namespace BackendSite.Service.Model.Common.UploadImage
{
    public class UploadImageResponse
    {
        public UploadImageResponse(string returnImagePath)
        {
            ReturnImagePath = returnImagePath;
        }
        
        public string ReturnImagePath { get; set; }
    }
}
