using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace RaceDay.API.Services;

/// <summary>
/// Stores private images in Azure Blob Storage. Only the API talks to Azure.
/// The database stores the stable Blob URI; MVC receives a short-lived read URL.
/// </summary>
public sealed class AzureBlobStorageService
{
    private readonly IConfiguration _configuration;
    public AzureBlobStorageService(IConfiguration configuration) => _configuration = configuration;

    private BlobContainerClient GetContainer()
    {
        var connection = _configuration["AzureStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Azure Blob Storage has not been configured.");
        var containerName = _configuration["AzureStorage:ContainerName"] ?? "raceday-images";
        return new BlobServiceClient(connection).GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(IFormFile file, string prefix)
    {
        var allowed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp"
        };
        if (file is null || file.Length == 0 || file.Length > 5 * 1024 * 1024 ||
            !allowed.TryGetValue(file.ContentType, out var extension))
            throw new ArgumentException("Upload a JPG, PNG or WebP image under 5 MB.");
        var container = GetContainer();
        await container.CreateIfNotExistsAsync(PublicAccessType.None);
        var name = $"{prefix}/{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(name);
        await using var input = file.OpenReadStream();
        await blob.UploadAsync(input, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        });
        return blob.Uri.ToString();
    }

    public string? GetReadUrl(string? savedBlobUri)
    {
        if (string.IsNullOrWhiteSpace(savedBlobUri)) return null;
        var container = GetContainer();
        var original = new Uri(savedBlobUri);
        var prefix = container.Uri.AbsolutePath.TrimEnd('/') + "/";
        if (!original.AbsolutePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(original.Host, container.Uri.Host, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Stored image URI is outside the configured container.");
        var blobName = Uri.UnescapeDataString(original.AbsolutePath[prefix.Length..]);
        var blob = container.GetBlobClient(blobName);
        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException("A storage account key is required for demo read URLs.");
        var sas = new BlobSasBuilder
        {
            BlobContainerName = container.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(60)
        };
        sas.SetPermissions(BlobSasPermissions.Read);
        return blob.GenerateSasUri(sas).ToString();
    }
}
