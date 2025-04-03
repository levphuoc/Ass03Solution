using BLL.Services.FirebaseServices.Core;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices.Utilities
{
    public static class FirebaseServiceUtils
    {
        public const string BucketName = "groupassignment03-prn222.firebasestorage.app";
        public static FirebaseStorageService CreateStorageService(string firebaseKeyPath)
        {
            return new FirebaseStorageService(firebaseKeyPath, BucketName);
        }

        public static class StoragePaths
        {
            public const string ProductsImagesFolder = "Products/Images/";
            public const string MembersAvatarFolder = "Members/Avatars/";

            public static string GetProductImagePath(string imageName)
                => $"{ProductsImagesFolder}{imageName}";

            public static string GetMemberAvatarPath(string avatarName)
                => $"{MembersAvatarFolder}{avatarName}";

            public static string GetSalesChartPath(DateTime dateGenerated, string chartType)
            => $"SalesReport/{dateGenerated:MMMddyy}/{chartType}.png";  
        }

        public static async Task<string> UploadBase64ImageAsync(string base64Image, string fileName, string folder = "SalesReport")
        {
            var imageBytes = Convert.FromBase64String(base64Image);
            using var stream = new MemoryStream(imageBytes);
            return await UploadFileAsync(stream, fileName, folder);
        }

        public static async Task<string> UploadFileAsync(Stream fileStream, string fileName, string firebaseKeyPath, string folder = "SalesReport")
        {
            string firebasePath = $"{folder}/{DateTime.Now:MMMddyy}/{fileName}";

            var firebaseService = FirebaseServiceUtils.CreateStorageService(firebaseKeyPath);
            return await firebaseService.UploadFileAsync(fileStream, firebasePath);
        }
        public static async Task<string> UploadSalesChartAsync(string firebaseKeyPath, Stream fileStream, DateTime dateGenerated, string chartType)
        {
            var service = CreateStorageService(firebaseKeyPath);
            var filePath = StoragePaths.GetSalesChartPath(dateGenerated, chartType);
            return await service.UploadFileAsync(fileStream, filePath);
        }
        public static async Task<string> UploadProductImageAsync(string firebaseKeyPath, Stream fileStream, string fileName)
        {
            var service = CreateStorageService(firebaseKeyPath);
            return await service.UploadFileAsync(fileStream, fileName, StoragePaths.ProductsImagesFolder);
        }

        public static async Task<string> UploadMemberAvatarAsync(string firebaseKeyPath, Stream fileStream, string fileName)
        {
            var service = CreateStorageService(firebaseKeyPath);
            return await service.UploadFileAsync(fileStream, fileName, StoragePaths.MembersAvatarFolder);
        }

        public static async Task DeleteFileAsync(string firebaseKeyPath, string fullPath)
        {
            var service = CreateStorageService(firebaseKeyPath);
            await service.DeleteFileAsync(fullPath);
        }

        public static string GetFilePublicUrl(string filePath)
            => $"https://storage.googleapis.com/{BucketName}/{filePath}";
    }

}
