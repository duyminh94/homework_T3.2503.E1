namespace homework.Services;

public class ImageStorageService
{
    private const long MaxImageSizeInBytes = 10 * 1024 * 1024;
    private const string UploadFolderName = "uploads";
    private const int HeaderLength = 12;

    private static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] RiffSignature = { 0x52, 0x49, 0x46, 0x46 };
    private static readonly byte[] WebpSignature = { 0x57, 0x45, 0x42, 0x50 };

    private readonly IWebHostEnvironment _environment;

    public ImageStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
            throw new InvalidImageException("Chưa chọn ảnh hoặc file ảnh bị rỗng");

        if (image.Length > MaxImageSizeInBytes)
            throw new InvalidImageException("Ảnh vượt quá 10 MB");

        var extension = await DetectImageExtensionAsync(image);
        if (extension == null)
            throw new InvalidImageException("Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP");

        var uploadFolderPath = Path.Combine(_environment.WebRootPath, UploadFolderName);
        Directory.CreateDirectory(uploadFolderPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolderPath, fileName);

        try
        {
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(fileStream);
        }
        catch
        {
            File.Delete(filePath);
            throw;
        }

        return $"/{UploadFolderName}/{fileName}";
    }

    public async Task<(byte[] Bytes, string MimeType)> ReadImageAsync(string imageUrl)
    {
        var imagePath = GetImagePath(imageUrl);
        var bytes = await File.ReadAllBytesAsync(imagePath);
        var mimeType = GetMimeType(Path.GetExtension(imagePath));

        return (bytes, mimeType);
    }

    public void DeleteImage(string imageUrl) => File.Delete(GetImagePath(imageUrl));

    private string GetImagePath(string imageUrl) => Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));

    private static string GetMimeType(string extension) => extension switch
    {
        ".jpg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => throw new InvalidImageException("Định dạng ảnh không được hỗ trợ")
    };

    private static async Task<string?> DetectImageExtensionAsync(IFormFile image)
    {
        var header = new byte[HeaderLength];

        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAtLeastAsync(header, HeaderLength, throwOnEndOfStream: false);
        if (bytesRead < HeaderLength)
            return null;

        if (header.AsSpan(0, JpegSignature.Length).SequenceEqual(JpegSignature))
            return ".jpg";

        if (header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return ".png";

        var isWebp = header.AsSpan(0, RiffSignature.Length).SequenceEqual(RiffSignature)
            && header.AsSpan(8, WebpSignature.Length).SequenceEqual(WebpSignature);
        if (isWebp)
            return ".webp";

        return null;
    }
}
