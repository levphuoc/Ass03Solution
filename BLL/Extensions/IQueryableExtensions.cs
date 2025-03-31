using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BLL.Extensions
{
    public static class IQueryableExtensions
    {
        public static IEnumerable<T> OrderByDynamic<T>(this IEnumerable<T> query, string sortColumn, bool descending)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var selector = Expression.PropertyOrField(parameter, sortColumn);
            var lambda = Expression.Lambda(selector, parameter);
            var methodName = descending ? "OrderByDescending" : "OrderBy";

            var result = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), selector.Type)
                .Invoke(null, new object[] { query, lambda.Compile() });

            return (IEnumerable<T>)result!;
        }
    }

}
