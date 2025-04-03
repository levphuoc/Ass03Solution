using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices.Core
{

    public static class FirebaseInitializerService
    {
        public static string AppName = "eStoreApp";
        private static FirebaseApp? _app;
        private static GoogleCredential? _credential;

        /// <summary>
        /// Initializes FirebaseApp once and stores GoogleCredential for reuse.
        /// </summary>
        public static FirebaseApp Initialize(string credentialsPath)
        {
            if (_app != null)
                return _app;

            _credential = GoogleCredential.FromFile(credentialsPath);

            _app = FirebaseApp.Create(new AppOptions
            {
                Credential = _credential
            }, AppName);

            return _app;
        }

        /// <summary>
        /// Returns stored credentials, or throws if not yet initialized.
        /// </summary>
        public static GoogleCredential GetCredential()
        {
            if (_credential == null)
                throw new InvalidOperationException("Firebase has not been initialized. Call Initialize() first.");

            return _credential;
        }

        /// <summary>
        /// Returns initialized FirebaseApp, or throws.
        /// </summary>
        public static FirebaseApp GetApp()
        {
            return _app ?? throw new InvalidOperationException("FirebaseApp is not initialized.");
        }
    }
}
