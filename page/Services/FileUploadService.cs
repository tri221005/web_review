namespace page.Services
{
    public interface IFileUploadService
    {
        Task<string?> UploadAsync(IFormFile file, string subfolder = "uploads");
        bool Delete(string relativePath);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> UploadAsync(IFormFile file, string subfolder = "uploads")
        {
            if (file == null || file.Length == 0) return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".glb" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext)) return null;

            var uploadsDir = Path.Combine(_env.WebRootPath, subfolder);
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return $"/{subfolder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                return null;
            }
        }

        public bool Delete(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
    }
}
