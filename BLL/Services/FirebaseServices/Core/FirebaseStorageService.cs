
using BLL.Services.FirebaseServices.Interfaces;
using BLL.Services.FirebaseServices.Utilities;
using FirebaseAdmin;
using Google.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;


namespace BLL.Services.FirebaseServices.Core
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(string firebaseKeyPath, string bucketName)
        {
            var credential = GoogleCredential.FromFile(firebaseKeyPath);
            _storageClient = StorageClient.Create(credential);
            _bucketName = bucketName;

        }
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            var objectName = $"{folderPath}{fileName}";
            await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
            return GetPublicUrl(objectName);
        }
        public async Task<string> UploadFileAsync(Stream fileStream, string filePath)
        {
            var objectName = $"{filePath}";
            await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
            return GetPublicUrl(objectName);
        }

        public async Task DeleteFileAsync(string fullPath)
        {
            await _storageClient.DeleteObjectAsync(_bucketName, fullPath);
        }

        public Task<string> GetFileUrlAsync(string fullPath)
        {
            return Task.FromResult(GetPublicUrl(fullPath));
        }

        private string GetPublicUrl(string objectName)
        {
            return FirebaseServiceUtils.GetFilePublicUrl(objectName);
        }
    }

}
