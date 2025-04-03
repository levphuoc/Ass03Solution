
namespace eStore.Utils
{
    using System.IO;
    using System.Threading.Tasks;
    using BLL.Services.IServices;
    using Microsoft.AspNetCore.Hosting;
    public class FileSaveAndLoadUtil : IFileSaveAndLoadUtil
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _defaultFolder;

        public FileSaveAndLoadUtil(IWebHostEnvironment environment, string folder = "images")
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _defaultFolder = folder;
        }

        #region Helpers

        private string GetFolderPath(string? customFolder = null)
        {
            return Path.Combine(_environment.WebRootPath, customFolder ?? _defaultFolder);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        #endregion

        #region Save Methods

        public async Task SaveBase64ImageAsync(string base64Data, string fileName, string? customFolder = null)
        {
            if (string.IsNullOrWhiteSpace(base64Data)) throw new ArgumentException("Base64 string cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name must be provided.");

            var folderPath = GetFolderPath(customFolder);
            EnsureDirectoryExists(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);
            var imageBytes = Convert.FromBase64String(base64Data);
            await File.WriteAllBytesAsync(fullPath, imageBytes);
        }

        public async Task SaveFormFileAsync(IFormFile file, string? fileName = null, string? customFolder = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var finalName = fileName ?? file.FileName;
            var folderPath = GetFolderPath(customFolder);
            EnsureDirectoryExists(folderPath);

            var fullPath = Path.Combine(folderPath, finalName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        #endregion

        #region Load Methods

        public async Task<byte[]?> LoadImageAsync(string fileName, string? customFolder = null)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            var fullPath = Path.Combine(GetFolderPath(customFolder), fileName);
            return File.Exists(fullPath) ? await File.ReadAllBytesAsync(fullPath) : null;
        }

        public async Task<byte[]?> LoadFileFromFullPathAsync(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath)) return null;
            return File.Exists(fullPath) ? await File.ReadAllBytesAsync(fullPath) : null;
        }

        #endregion

        #region File URL & Info

        public string GetFileUrl(string fileName, string? customFolder = null)
        {
            var folder = customFolder ?? _defaultFolder;
            return $"/{folder}/{fileName}";
        }

        public bool FileExists(string fileName, string? customFolder = null)
        {
            var fullPath = Path.Combine(GetFolderPath(customFolder), fileName);
            return File.Exists(fullPath);
        }

        public List<string> ListAllFileNames(string? customFolder = null)
        {
            var folderPath = GetFolderPath(customFolder);
            return Directory.Exists(folderPath)
                ? Directory.GetFiles(folderPath).Select(Path.GetFileName).ToList()
                : new List<string>();
        }

        #endregion

        #region Delete Methods

        public bool DeleteFile(string fileName, string? customFolder = null)
        {
            var fullPath = Path.Combine(GetFolderPath(customFolder), fileName);
            if (!File.Exists(fullPath)) return false;

            File.Delete(fullPath);
            return true;
        }

        public bool DeleteFileByFullPath(string fullPath)
        {
            if (!File.Exists(fullPath)) return false;

            File.Delete(fullPath);
            return true;
        }

        #endregion
    }

}
