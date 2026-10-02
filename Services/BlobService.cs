using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace CinemaApp.Services;

public interface IBlobService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task DeleteAsync(string blobName);
    string GetSasUrl(string blobName);
    Task<string?> ReadTextAsync(string blobName);
    Task WriteTextAsync(string blobName, string content);
}

public class BlobService : IBlobService
{
    private readonly BlobContainerClient _container;

    public BlobService(IConfiguration config)
    {
        var cs = config["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("AzureBlob:ConnectionString не налаштовано");

        _container = new BlobContainerClient(cs, config["AzureBlob:ContainerName"] ?? "cinema");
    }

    private Task EnsureContainerAsync()
        => _container.CreateIfNotExistsAsync(PublicAccessType.None);

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        await EnsureContainerAsync();

        var blobName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blob = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        });
        return blobName;
    }

    public async Task DeleteAsync(string blobName)
    {
        if (string.IsNullOrEmpty(blobName)) return;
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync();
    }
    public string GetSasUrl(string blobName)
        => _container.GetBlobClient(blobName)
            .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))
            .ToString();

    public async Task<string?> ReadTextAsync(string blobName)
    {
        try
        {
            var result = await _container.GetBlobClient(blobName).DownloadContentAsync();
            return result.Value.Content.ToString();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
    public async Task WriteTextAsync(string blobName, string content)
    {
        await EnsureContainerAsync();
        await _container.GetBlobClient(blobName)
            .UploadAsync(BinaryData.FromString(content), overwrite: true);
    }
}
