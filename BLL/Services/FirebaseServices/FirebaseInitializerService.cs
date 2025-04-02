using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices
{
    public static class FirebaseInitializerService
    {
        private static bool _isInitialized;

        public static void Initialize(string credentialsPath)
        {
            if (_isInitialized) return;

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialsPath)
            });

            _isInitialized = true;
        }
    }

}
