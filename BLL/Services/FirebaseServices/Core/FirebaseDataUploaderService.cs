using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Extensions;
using BLL.Services.FirebaseServices.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;

namespace BLL.Services.FirebaseServices.Core
{
    public class FirebaseDataUploaderService : IFirebaseDataUploaderService
    {
        private readonly FirestoreDb _firestore;

        public FirebaseDataUploaderService(string projectId, GoogleCredential credential)
        {
            _firestore = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();
        }

        public async Task UploadReportWithProductsAsync(DateTime startDate, DateTime endDate, List<SalesReportDTO> reportData)
        {
            if (reportData == null || reportData.Count == 0)
            {
                Console.WriteLine("⚠️ No sales report data to upload.");
                return;
            }

            try
            {
                var reportDocRef = await _firestore.Collection("SalesReports").AddAsync(new
                {
                    StartDate = startDate.EnsureUtc(),
                    EndDate = endDate.EnsureUtc(),
                    CreatedAt = Timestamp.GetCurrentTimestamp()
                });

                var productsCollection = reportDocRef.Collection("Products");

                var batch = _firestore.StartBatch();

                foreach (var item in reportData)
                {
                    var productData = new Dictionary<string, object>
                    {
                        { "ProductName", item.ProductName },
                        { "TotalQuantity",Convert.ToDouble(item.TotalQuantity)},
                        { "TotalRevenue", Convert.ToDouble(item.TotalRevenue) }
                    };

                    batch.Create(productsCollection.Document(), productData);
                    Console.WriteLine($"📦 Queued: {item.ProductName}");
                }

                await batch.CommitAsync();
                Console.WriteLine("🎉 All sales data successfully uploaded to Firestore.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Error uploading to Firestore: {ex.Message}");
            }
        }
    }


}
