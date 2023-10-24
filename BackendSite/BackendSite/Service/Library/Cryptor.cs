using System.Security.Cryptography;
using System.Text;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Model;

namespace BackendSite.Service.Library;

public class Cryptor
{
    private readonly string _key;
    private readonly string _iv;

    public Cryptor(IServiceProvider provider)
    {
        var configBll = provider.GetRequiredService<ConfigBLL>();
        var config = configBll.GetConfigValue<CryptorConfig>("CryptorConfig");
        _key = config.Key;
        _iv = config.IV;
    }

    public string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }
        var key = Encoding.ASCII.GetBytes(_key);
        var iv = Encoding.ASCII.GetBytes(_iv);
        var bytes = Encoding.UTF8.GetBytes(text);

        using var aes = Aes.Create();
        var ict = aes.CreateEncryptor(key, iv);
        var result = ict.TransformFinalBlock(bytes, 0, bytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var key = Encoding.ASCII.GetBytes(_key);
        var iv = Encoding.ASCII.GetBytes(_iv);
        var bytes = Convert.FromBase64String(text);

        using var aes = Aes.Create();
        var cryptoTransform = aes.CreateDecryptor(key, iv);
        var result = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);

        return Encoding.UTF8.GetString(result);
    }
}