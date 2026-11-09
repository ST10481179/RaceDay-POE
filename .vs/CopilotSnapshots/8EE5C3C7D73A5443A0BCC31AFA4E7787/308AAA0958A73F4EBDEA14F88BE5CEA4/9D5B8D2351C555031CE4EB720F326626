using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;

namespace RaceDay.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private const long MaxBytes = 2 * 1024 * 1024; // 2 MB

        private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        private readonly BlobContainerClient? _container;
        private readonly string _fallbackFolder;

        public BlobStorageService(IConfiguration config, IWebHostEnvironment environment)
        {
            _fallbackFolder = Path.Combine(environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads", "banners");

            var connectionString = config["AzureStorage:ConnectionString"];
            var containerName = config["AzureStorage:ContainerName"] ?? "event-banners";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            try
            {
                _container = new BlobContainerClient(connectionString.Trim(), containerName);
            }
            catch (FormatException)
            {
                var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=')[0].Trim());
                throw new InvalidOperationException(
                    $"Connection string is invalid. Length={connectionString.Length}; " +
                    $"starts with '{connectionString[..Math.Min(25, connectionString.Length)]} '; " +
                    $"keys found: {string.Join(", ", parts)}");
            }
        }

        public string? Validate(IFormFile file)
        {
            if (file.Length == 0)
            {
                return "The selected file is empty.";
            }

            if (file.Length > MaxBytes)
            {
                return "The banner image must be less than 2 MB in size.";
            }

            var extension = Path.GetExtension(file.FileName);

            if (!AllowedTypes.TryGetValue(extension, out var expectedType) ||
                !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
            {
                return "Only JPG, PNG or WebP images are allowed.";
            }

            using var stream = file.OpenReadStream();
            Span<byte> header = stackalloc byte[12];
            var read = stream.Read(header);

            var valid = expectedType switch
            {
                "image/jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                "image/png" => read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
                "image/webp" => read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                    && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
                _ => false
            };

            return valid ? null : "The selected file is not a valid image.";
        }

        public async Task<BlobUploadResult> UploadBannerAsync(IFormFile file, CancellationToken ct = default)
        {
            if (_container is not null)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var blobName = $"banners/{Guid.NewGuid():N}{ext}";
                var blob = _container.GetBlobClient(blobName);

                await using var stream = file.OpenReadStream();
                await blob.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = AllowedTypes[ext],
                        CacheControl = "public, max-age=31536000"
                    }
                }, ct);

                return new BlobUploadResult(blobName, blob.Uri.ToString());
            }

            Directory.CreateDirectory(_fallbackFolder);
            var ext2 = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext2}";
            var targetPath = Path.Combine(_fallbackFolder, fileName);

            await using (var stream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream, ct);
            }

            return new BlobUploadResult(fileName, $"/uploads/banners/{fileName}");
        }

        public async Task DeleteAsync(string? blobName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return;
            }

            if (_container is not null)
            {
                await _container.DeleteBlobIfExistsAsync(blobName, cancellationToken: ct);
                return;
            }

            var filePath = Path.Combine(_fallbackFolder, blobName.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath), ct);
            }
        }
    }
}

