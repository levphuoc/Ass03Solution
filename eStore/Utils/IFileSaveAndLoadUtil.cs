using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eStore.Utils
{
    public interface IFileSaveAndLoadUtil
    {
        Task SaveBase64ImageAsync(string base64Data, string fileName, string? customFolder = null);
        Task SaveFormFileAsync(IFormFile file, string? fileName = null, string? customFolder = null);

        Task<byte[]?> LoadImageAsync(string fileName, string? customFolder = null);
        Task<byte[]?> LoadFileFromFullPathAsync(string fullPath);

        string GetFileUrl(string fileName, string? customFolder = null);
        bool FileExists(string fileName, string? customFolder = null);
        List<string> ListAllFileNames(string? customFolder = null);

        bool DeleteFile(string fileName, string? customFolder = null);
        bool DeleteFileByFullPath(string fullPath);
    }



}
