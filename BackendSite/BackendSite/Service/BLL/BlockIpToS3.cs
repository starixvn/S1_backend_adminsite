using Amazon.S3.Model;

using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL;

public class BlockIpToS3
{
    private readonly AWSS3Helper _s3Helper;
    private readonly BlockService _blockService;
    private readonly string _fileName;
    private readonly string _folder;
    private readonly string _nginxIpKey;
    private readonly string _bucketName;

    public BlockIpToS3(AWSS3Helper s3Helper, IWebHostEnvironment env, BlockService blockService, ConfigBLL configBll)
    {
        _s3Helper = s3Helper;
        _blockService = blockService;
        _bucketName = configBll.GetConfigValue("AWSConfigBucketName");
        _fileName = env.IsProduction() || env.EnvironmentName == "Release" ? "geoblockIp.txt" : "geoblockIp_test.txt";
        _nginxIpKey = env.IsProduction() || env.EnvironmentName == "Release" ? "$remote_addr" : "$real_ip";
        _folder = Path.Combine(env.ContentRootPath, "NginxConfig");
    }
    public async Task<int> Upload()
    {
        Directory.CreateDirectory(_folder);
        var filePath = Path.Combine(_folder, _fileName);
        var blockIps = await _blockService.GetBlockIPList<BlockIPForUpload>();
        var ipLines = blockIps.Select(a => a.IP).ToList();
        await File.WriteAllTextAsync(filePath, "#### Block List 0:open 1:block -1:default\r\n" +
                                               $"geo {_nginxIpKey} $ipcheck " +
                                               "{ \r\n" +
                                               "default -1;\r\n");
        await File.AppendAllLinesAsync(filePath, ipLines);
        await File.AppendAllTextAsync(filePath, "}\r\n" +
                                                "\r\n" +
                                                "map $ip_geoblock:$ipcheck $geoblock {\r\n" +
                                                "default $default_geoblock;\r\n" +
                                                "0:0 0;\r\n" +
                                                "0:1 0;\r\n" +
                                                "0:-1 0;\r\n" +
                                                "-1:1 1;\r\n" +
                                                "}\r\n");

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
            BucketName = _bucketName,
            ContentType = "text/plain",
            InputStream = destination,
            Key = _fileName
        };
        var putResponse = await _s3Helper.PutObjectAsync(putRequest);
        return !putResponse.HttpStatusCode.IsSuccess() ? 0 : 1;
    }
}