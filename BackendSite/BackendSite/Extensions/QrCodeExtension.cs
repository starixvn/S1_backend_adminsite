using BarcodeReader.ImageSharp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BackendSite.Extensions;

public static class QrCodeExtension
{
    public static async Task<string> DecodeFromImage(IFormFile file)
    {
        if (file == null || !file.ContentType.StartsWith("image/"))
        {
            return null;
        }

        await using var fileStream = file.OpenReadStream();
        var image = await Image.LoadAsync<Rgba32>(fileStream);
        var reader = new BarcodeReader<Rgba32>(types: ZXing.BarcodeFormat.QR_CODE);
        var result = await reader.DecodeAsync(image);

        return result.Value;
    }
}