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
        _fileName = env.IsProduction() ? "blockIp.txt" : "blockIp_test.txt";
        _folder = Path.Combine(env.ContentRootPath, "NginxConfig");
    }
    public async Task<int> Upload(string ip)
    {
        var filePath = Path.Combine(_folder, _fileName);
        //檢查檔案存不存在
        if (!File.Exists(filePath))
            await InitialConfig(filePath);
        
        //檢查IP是否存在
        if (IsIpExist(filePath, ip))
            return 0;
        await AddIp(filePath, ip);
        
        return await UploadToS3(filePath);
    }

    public async Task<int> DeleteIp(string ip)
    {
        var filePath = Path.Combine(_folder, _fileName);
        //檢查檔案存不存在
        if (!File.Exists(filePath))
            await InitialConfig(filePath);

        await DeleteIp(filePath, ip);
        
        return await UploadToS3(filePath);;
    }

    private async Task InitialConfig(string filePath)
    {
        Directory.CreateDirectory(_folder);
        var blockIps = await _blockService.GetBlockIPList();
        await using var outputFile = new StreamWriter(filePath);
        foreach (var ip in blockIps)
            await outputFile.WriteLineAsync(ip.IP);
    }

    private bool IsIpExist(string filePath, string ip)
    {
        return File.ReadLines(filePath).Any(line => string.Equals(line, ip));
    }

    private async Task AddIp(string filePath, string ip)
    {
        await using var outputFile = new StreamWriter(filePath, true);
        await outputFile.WriteLineAsync(ip);
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

    private async Task DeleteIp(string filePath, string ip)
    {
        var temp = (await File.ReadAllLinesAsync(filePath)).Where(blockIp => blockIp != ip).ToList();
        if (temp != null) await File.WriteAllLinesAsync(filePath, temp);
    }
}