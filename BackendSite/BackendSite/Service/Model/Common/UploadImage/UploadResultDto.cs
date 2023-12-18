namespace BackendSite.Service.Model.Common.UploadImage
{
    public class UploadResultDto
    {
        public UploadResultDto(bool success,string imagePath, string message)
        {
            Success = success;
            ImagePath = imagePath;
            Message = message;
        }

        public bool Success { get; set; }

        public string ImagePath { get; set; }

        public string Message { get; set; }
    }
}
