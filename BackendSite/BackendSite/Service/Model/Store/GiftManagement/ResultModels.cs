namespace BackendSite.Service.Model.Store.GiftManagement
{
    public class UploadPictureResult
    {
        public UploadPictureResult(string picUrl)
        {
            PictureUrl = picUrl;
        }
        public string PictureUrl { get; set; }
    }
}