using Uni_Selector.Service.Interface;

namespace Uni_Selector.Service.Implementation
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is empty");
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!IsValidFileType(file, allowedExtensions))
            {
                throw new ArgumentException("Invalid image file type");
            }

            if (!IsValidFileSize(file, 5 * 1024 * 1024))
            {
                throw new ArgumentException("Image file is too large (max 5MB)");
            }

            return await UploadFileAsync(file, folder);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"File deleted: {filePath}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }

        public bool IsValidFileType(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(IFormFile file, long maxSizeInBytes)
        {
            return file.Length <= maxSizeInBytes;
        }
    }

}
