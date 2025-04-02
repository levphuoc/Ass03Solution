using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services.IServices;
using Google.Cloud.Firestore;

namespace BLL.Services.FirebaseServices
{
    public class FirebaseDataUploaderService : IFirebaseDataUploaderService
    {
        private readonly FirestoreDb _firestore;

        public FirebaseDataUploaderService(string firebaseKeyPath, string projectId)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseKeyPath);
            _firestore = FirestoreDb.Create(projectId);
        }
        public async Task UploadSalesReportAsync(DateTime startDate, DateTime endDate, List<SalesReportDTO> reportData)
        {
            var reportDoc = await _firestore.Collection("SalesReports").AddAsync(new
            {
                StartDate = startDate,
                EndDate = endDate,
                CreatedAt = Timestamp.GetCurrentTimestamp()
            });

            var productsCollection = reportDoc.Collection("Products");

            foreach (var item in reportData)
            {
                await productsCollection.AddAsync(new
                {
                    item.ProductName,
                    item.TotalQuantity,
                    item.TotalRevenue
                });
            }
        }
        public async Task UploadSalesReportWithSubcollectionAsync(DateTime startDate, DateTime endDate, List<SalesReportDTO> reportData)
        {
            try
            {
                var reportDocRef = await _firestore.Collection("SalesReports").AddAsync(new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    CreatedAt = Timestamp.GetCurrentTimestamp()
                });

                var productsCollection = reportDocRef.Collection("Products");

                foreach (var item in reportData)
                {
                    var productDoc = new Dictionary<string, object>
                    {
                        { "ProductName", item.ProductName },
                        { "TotalQuantity", item.TotalQuantity },
                        { "TotalRevenue", item.TotalRevenue }
                    };

                    await productsCollection.AddAsync(productDoc);
                    Console.WriteLine($"✅ Added product: {item.ProductName}");
                }

                Console.WriteLine("🎉 All products uploaded to Firestore successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Firestore subcollection upload failed: {ex.Message}");
            }
        }

        public async Task UploadSalesReportAsync(List<SalesReportDTO> reportData, DateTime start, DateTime end)
        {
            var collectionRef = _firestore.Collection("SalesReports");
            var docRef = collectionRef.Document($"{start:yyyyMMdd}-{end:yyyyMMdd}");

            var salesData = reportData.Select(r => new
            {
                r.ProductName,
                r.TotalQuantity,
                r.TotalRevenue
            }).ToList();

            var reportDoc = new
            {
                startDate = start,
                endDate = end,
                generatedAt = DateTime.UtcNow,
                items = salesData
            };

            await docRef.SetAsync(reportDoc);
        }
    }

}
