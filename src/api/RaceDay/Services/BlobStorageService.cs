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

private readonly BlobContainerClient container;


public BlobStorageService(IConfiguration config)
        {
            var connectionString = config["AzureStorage:ConnectionString"];
            var containerName = config["AzureStorage:ContainerName"] ?? "event-banners";

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");

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
            if (file.Length == 0) return "The selected file is empty.";

        }
    }
}