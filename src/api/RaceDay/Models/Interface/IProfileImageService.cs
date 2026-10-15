using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace RaceDay.Models.Interface
{
    public interface IProfileImageService
    {
        Task<string> SaveAsync(IFormFile file);

        void Delete(string? relativePath);
    }
    public class ProfileImageService : IProfileImageService
    {
        private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".gif" };

        private const long Maxbytes = 2 * 1024 * 1024; // 2 MB

        private readonly IWebHostEnvironment _env;

        public ProfileImageService(IWebHostEnvironment env) => _env = env;

        public async Task<string> SaveAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Allowed.Contains(ext) || file.Length == 0 || file.Length > Maxbytes)
                throw new ArgumentException("Invalid file", nameof(file));

            var folder = Path.Combine(_env.WebRootPath,"uploads","profiles");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/profiles/{fileName}";
        }

        public void Delete(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var filePath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}