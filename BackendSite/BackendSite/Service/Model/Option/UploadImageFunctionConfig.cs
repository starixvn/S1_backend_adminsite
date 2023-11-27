namespace BackendSite.Service.Model.Option
{
    public class UploadImageFunctionConfig
    {
        public string EnvironmentPrefix { get; set; }

        public List<UploadFunction> UploadFunctions { get; set; }
    }

    public class UploadFunction
    {
        public int UploadFunctionType { get; set; }

        public string UploadFolder { get; set; }

        public List<string> AllowFileExtension { get; set; }
    }
}
