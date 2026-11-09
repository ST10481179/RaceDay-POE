using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Services;

public class BlobStorageService : IBlobStorageService
{
    private const long MaxBytes = 2 * 1024 * 1024; // 2 MB

    private static readonly Dictionary<string, string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureStorage:ConnectionString"];
        var containerName = config["AzureStorage:ContainerName"] ?? "event-banners";

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is not configured.");
        }

        try
        {
            _container = new BlobContainerClient(connectionString.Trim(), containerName);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is invalid.", ex);
        }
    }

    public string? Validate(IFormFile file)
    {
        if (file is null)
            return "A file must be selected.";

        if (file.Length == 0)
            return "The selected file is empty.";

        if (file.Length > MaxBytes)
            return "The selected file must be no larger than 2 MB.";

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedTypes.TryGetValue(extension, out var expectedContentType))
            return "Only JPG, PNG, and WebP images are allowed.";

        if (!string.Equals(file.ContentType, expectedContentType,
                StringComparison.OrdinalIgnoreCase))
            return "The file type does not match its extension.";

        return null;
    }

    public async Task<BlobUploadResult> UploadBannerAsync(
        IFormFile file, CancellationToken ct = default)
    {
        var validationError = Validate(file);
        if (validationError is not null)
            throw new ArgumentException(validationError, nameof(file));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"{Guid.NewGuid():N}{extension}";
        var contentType = AllowedTypes[extension];

        await _container.CreateIfNotExistsAsync(cancellationToken: ct);

        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(
            file.OpenReadStream(),
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            },
            ct);

        return new BlobUploadResult(blobName, blob.Uri.ToString());
    }

    public async Task DeleteAsync(string blobName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentException("A blob name is required.", nameof(blobName));

        await _container.GetBlobClient(blobName)
            .DeleteIfExistsAsync(cancellationToken: ct);
    }
}
