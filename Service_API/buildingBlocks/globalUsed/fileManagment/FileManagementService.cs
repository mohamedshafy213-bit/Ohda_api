using Service_API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Service_API.Services.FileManagement
{
    public class FileManagementService : IFileManagementService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUploadPath;

        public FileManagementService(IConfiguration configuration, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;

            try
            {
                _baseUploadPath = Path.Combine(_environment.WebRootPath ?? Directory.GetCurrentDirectory(), "uploads");
                EnsureDirectoryExists(Path.Combine(_baseUploadPath, "uploads"));
                EnsureDirectoryExists(Path.Combine(_baseUploadPath, "images"));
            }
            catch (Exception ex)
            {
                // Log or throw with details
                throw new InvalidOperationException($"Failed to initialize FileManagementService: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName = "uploads")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (!IsValidFile(file))
                throw new ArgumentException("Invalid file type");

            var fileName = await GenerateUniqueFileNameAsync(file.FileName);
            var folderPath = Path.Combine(_baseUploadPath, folderName);
            var filePath = Path.Combine(folderPath, fileName);

            EnsureDirectoryExists(folderPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<string> UploadImageAsync(IFormFile image, string folderName = "images")
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Image is empty or null");

            if (!IsValidImage(image))
                throw new ArgumentException("Invalid image type");

            var fileName = await GenerateUniqueFileNameAsync(image.FileName);
            var folderPath = Path.Combine(_baseUploadPath, folderName);
            var filePath = Path.Combine(folderPath, fileName);

            EnsureDirectoryExists(folderPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<byte[]> GetFileAsync(string fileName, string folderName = "uploads")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {fileName} not found");

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<byte[]> GetImageAsync(string fileName, string folderName = "images")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Image {fileName} not found");

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderName = "uploads")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteImageAsync(string fileName, string folderName = "images")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        public async Task<string> GetFileUrlAsync(string fileName, string folderName = "uploads")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {fileName} not found");

            // Get current request URL dynamically
            var baseUrl = GetCurrentBaseUrl();
            return $"{baseUrl}/uploads/{folderName}/{fileName}";
        }

        public async Task<string> GetImageUrlAsync(string fileName, string folderName = "images")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);
            // Get current request URL dynamically
            var baseUrl = GetCurrentBaseUrl();

            if (!File.Exists(filePath))
                return $"{baseUrl}/uploads/default.png";

            return $"{baseUrl}/uploads/{folderName}/{fileName}";
        }

        public async Task<string> GetImageApiUrlAsync(string fileName, string folderName = "images")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Image {fileName} not found");

            // Get current request URL dynamically
            var baseUrl = GetCurrentBaseUrl();
            return $"{baseUrl}/files/image/{folderName}/{fileName}";
        }

        public async Task<string> GetFileApiUrlAsync(string fileName, string folderName = "uploads")
        {
            var filePath = Path.Combine(_baseUploadPath, folderName, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {fileName} not found");

            // Get current request URL dynamically
            var baseUrl = GetCurrentBaseUrl();
            return $"{baseUrl}/files/download/{folderName}/{fileName}";
        }

        public async Task<List<string>> GetAllFilesAsync(string folderName = "uploads")
        {
            var folderPath = Path.Combine(_baseUploadPath, folderName);

            if (!Directory.Exists(folderPath))
                return new List<string>();

            var files = Directory.GetFiles(folderPath);
            return files.Select(Path.GetFileName).ToList();
        }

        public async Task<List<string>> GetAllImagesAsync(string folderName = "images")
        {
            var folderPath = Path.Combine(_baseUploadPath, folderName);

            if (!Directory.Exists(folderPath))
                return new List<string>();

            var files = Directory.GetFiles(folderPath);
            return files.Select(Path.GetFileName).ToList();
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",".svg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }

        public bool IsValidFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] {
                ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx",
                ".ppt", ".pptx", ".zip", ".rar", ".jpg", ".jpeg",
                ".png", ".gif", ".bmp", ".webp"
            };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }

        private async Task<string> GenerateUniqueFileNameAsync(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

            // Generate a unique hash based on timestamp and original filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var hash = await GenerateHashAsync($"{timestamp}_{fileNameWithoutExtension}");

            return $"{hash}{extension}";
        }

        private async Task<string> GenerateHashAsync(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = await Task.Run(() => sha256.ComputeHash(bytes));
                return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Substring(0, 16);
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string GetCurrentBaseUrl()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var request = httpContext.Request;
                var scheme = request.Scheme;
                var host = request.Host.Value;
                return $"{scheme}:/{host}";
            }

            // Fallback to configuration if no HTTP context is available
            return _configuration["BaseUrl"] ?? "https://localhost:44395";
        }
    }
}