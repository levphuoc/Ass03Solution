using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices.Interfaces
{
    public interface IFirebaseAnalyticsService
    {
        Task LogEventAsync(string eventName, Dictionary<string, object>? parameters = null);
    }

}
