namespace Uni_Selector.Service.Interface
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        Task<string> UploadImageAsync(IFormFile file, string folder);
        bool IsValidFileType(IFormFile file, string[] allowedExtensions);
        bool IsValidFileSize(IFormFile file, long maxSizeInBytes);
    }
}
