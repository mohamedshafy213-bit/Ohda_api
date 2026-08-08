using Microsoft.AspNetCore.Http;

namespace Service_API.Services.Interfaces
{
    public interface IFileManagementService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName = "uploads");
        Task<string> UploadImageAsync(IFormFile image, string folderName = "images");
        Task<byte[]> GetFileAsync(string fileName, string folderName = "uploads");
        Task<byte[]> GetImageAsync(string fileName, string folderName = "images");
       Task<bool> DeleteFileAsync(string fileName, string folderName = "uploads");
        Task<bool> DeleteImageAsync(string fileName, string folderName = "images");
        Task<string> GetFileUrlAsync(string fileName, string folderName = "uploads");
        Task<string> GetImageUrlAsync(string fileName, string folderName = "images");
       /// Task<string> GetFileApiUrlAsync(string fileName, string folderName = "uploads");
       // Task<string> GetImageApiUrlAsync(string fileName, string folderName = "images");
       // Task<List<string>> GetAllFilesAsync(string folderName = "uploads");
        Task<List<string>> GetAllImagesAsync(string folderName = "images");
        bool IsValidImage(IFormFile file);
        bool IsValidFile(IFormFile file);
    }
} 