using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events
{
    internal class Utilities
    {
        internal static readonly Task CompletedTask = Task.FromResult(true);

        internal static MethodInfo GetOpenGenericMethodInfo<T>(Expression<Action<T>> expression)
        {
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression != null)
            {
                string methodName = methodCallExpression.Method.Name;
                MethodInfo methodInfo = typeof(T).GetRuntimeMethods().FirstOrDefault(m => m.Name == methodName);
                return methodInfo;
            }

            return null;
        }
    }
}
