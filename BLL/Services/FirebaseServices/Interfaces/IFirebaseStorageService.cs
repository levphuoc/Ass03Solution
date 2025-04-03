using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices.Interfaces
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath);
        Task<string> UploadFileAsync(Stream fileStream, string filePath);
        Task DeleteFileAsync(string fullPath);
        Task<string> GetFileUrlAsync(string fullPath);
    }

}
