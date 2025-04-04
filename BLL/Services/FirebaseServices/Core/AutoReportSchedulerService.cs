using BLL.Services.FirebaseServices.Interfaces;
using BLL.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices.Core
{
    public class AutoReportSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoReportSchedulerService> _logger;
        private readonly TimeSpan _dailyRunTime = new TimeSpan(8, 0, 0); // 08:00 AM

        public AutoReportSchedulerService(IServiceProvider serviceProvider, ILogger<AutoReportSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRunTime = DateTime.Today.Add(_dailyRunTime);
                if (now > nextRunTime) nextRunTime = nextRunTime.AddDays(1);

                var delay = nextRunTime - now;
                _logger.LogInformation($"Next auto-report scheduled in {delay.TotalMinutes} minutes...");
                await Task.Delay(delay, stoppingToken);

                await GenerateAndNotifyAsync();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // run every 24 hours
            }
        }

        private async Task GenerateAndNotifyAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                var salesService = scope.ServiceProvider.GetRequiredService<ISalesReportService>();
                var firebaseUploader = scope.ServiceProvider.GetRequiredService<IFirebaseDataUploaderService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-1); // daily report

                var reportData = await salesService.GenerateReportAsync(startDate, endDate);
                await firebaseUploader.UploadReportWithProductsAsync(startDate, endDate, reportData);

                var body = $"Auto-report generated and saved to Firestore for {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}.";
                await emailService.SendEmailAsync("admin@example.com", " Daily Sales Report", body);

                _logger.LogInformation("Auto-report sent and email notified.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Auto-report failed.");
            }
        }
    }

}
