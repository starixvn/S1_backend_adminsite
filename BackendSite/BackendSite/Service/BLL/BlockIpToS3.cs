using Amazon.S3.Model;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;

namespace BackendSite.Service.BLL;

public class BlockIpToS3
{
    private readonly AWSS3Helper _s3Helper;
    private readonly BlockService _blockService;
    private readonly string _fileName;
    private readonly string _folder;

    public BlockIpToS3(AWSS3Helper s3Helper, IWebHostEnvironment env, BlockService blockService)
    {
        _s3Helper = s3Helper;
        _blockService = blockService;
        _fileName = env.IsProduction() || env.EnvironmentName == "Release" ? "blockIp.txt" : "blockIp_test.txt";
        _folder = Path.Combine(env.ContentRootPath, "NginxConfig");
    }
    public async Task<int> Upload()
    {
        var filePath = Path.Combine(_folder, _fileName);
        var blockIps = await _blockService.GetBlockIPList();
        var ipLines = blockIps.Select(a => a.IP).ToList();
        await File.WriteAllLinesAsync(filePath, ipLines);

        return await UploadToS3(filePath);
    }

    private async Task<int> UploadToS3(string filePath)
    {
        var destination = new MemoryStream();
        await using (var source = File.Open(filePath, FileMode.Open))
        {
            await source.CopyToAsync(destination);
        }
        var putRequest = new PutObjectRequest
        {
            BucketName = "ailab-config",
            ContentType = "text/plain",
            InputStream = destination,
            Key = _fileName
        };
        var putResponse = await _s3Helper.PutObjectAsync(putRequest);
        return !putResponse.HttpStatusCode.IsSuccess() ? 0 : 1;
    }
}