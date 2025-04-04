using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime EnsureUtc(this DateTime dt)
        {
            return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
        }
    }

}
