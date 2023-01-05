using BackendSite.ExceptionHandling;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using LazZiya.ImageResize.ResizeMethods;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Linq.Expressions;
using System.Reflection;
using Amazon.CloudFront.Model;
using YamlDotNet.Core.Tokens;


namespace BackendSite.Extensions
{
    public static class EnumerableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IEnumerable<T> source, IOrderBy orderBy)
        {
            var methodName = orderBy.OrderByDirection == OrderByDirection.Ascending ? "OrderBy" : "OrderByDescending";
            return ApplyOrder(source.AsQueryable(), orderBy.OrderByProperty, methodName);
        }

        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            try
            {
                Type type = typeof(T);
                ParameterExpression arg = Expression.Parameter(type, "obj");
                Expression expr = arg;

                PropertyInfo pi = type.GetProperties().Single(i => property.Equals(i.Name, StringComparison.OrdinalIgnoreCase));
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;

                Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
                LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

                var o = typeof(Queryable).GetMethods().Single(
                        method => method.Name == methodName
                                && method.IsGenericMethodDefinition
                                && method.GetGenericArguments().Length == 2
                                && method.GetParameters().Length == 2).MakeGenericMethod(typeof(T), type);
                var result = (IOrderedQueryable<T>)o.Invoke(null, new object[] { source, lambda });
                return result;
            }
            catch (Exception)
            {
                throw new AdminException(ErrorCodeEnum.OrderByInvalid);
            }
        }

        public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, int index, int count)
        {
            return source.Skip(index).Take(count);
        }
    }
}
