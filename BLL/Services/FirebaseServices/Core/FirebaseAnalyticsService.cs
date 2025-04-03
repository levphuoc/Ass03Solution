using System.Net.Http.Json;
using BLL.Services.FirebaseServices.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Services.FirebaseServices.Core
{
   
    public class FirebaseAnalyticsService : IFirebaseAnalyticsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _firebaseApiSecret;
        private readonly string _firebaseMeasurementId;
        private readonly string _clientId;
        private readonly ILogger<FirebaseAnalyticsService> _logger;

        public FirebaseAnalyticsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<FirebaseAnalyticsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _firebaseApiSecret = configuration["Firebase:ApiSecret"] ?? throw new ArgumentNullException("Firebase:ApiSecret");
            _firebaseMeasurementId = configuration["Firebase:MeasurementId"] ?? throw new ArgumentNullException("Firebase:MeasurementId");

            // Could also store/load client ID from session/localstorage for real users
            _clientId = Guid.NewGuid().ToString();
        }

        public async Task LogEventAsync(string eventName, Dictionary<string, object>? parameters = null)
        {
            try
            {
                var resolvedParams = parameters ?? new Dictionary<string, object>();

                var payload = new
                {
                    client_id = _clientId,
                    events = new[]
                    {
                new
                {
                    name = eventName,
                    @params = resolvedParams
                }
            }
                };

                string endpoint = $"https://www.google-analytics.com/mp/collect?measurement_id={_firebaseMeasurementId}&api_secret={_firebaseApiSecret}";
                var response = await _httpClient.PostAsJsonAsync(endpoint, payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Firebase Analytics failed: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in FirebaseAnalyticsService.LogEventAsync");
            }
        }

    }

}
