using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Arpa.Utilities
{
	public static class ReflectionUtilities
	{
		private static ConcurrentDictionary<MethodInfo, Func<object, object[], object>> _methodInvocationCache = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();

		public static Func<object, object[], object> CreateCompiledInvocationDelegate(this MethodInfo info)
		{
			if (_methodInvocationCache.TryGetValue(info, out var cachedDelegate))
				return cachedDelegate;

			var instParamExpr = Expression.Parameter(typeof(object), "instance");
			var argsParamExpr = Expression.Parameter(typeof(object[]), "args");

			int index = 0;
			var argsExtractionExpr = info.GetParameters()
			   .Select(parameter =>
				  Expression.Convert(
					 Expression.ArrayAccess(
						argsParamExpr,
						Expression.Constant(index++)
					 ),
					 parameter.ParameterType
				  )
			   ).ToList();

			var callExpr = Expression.Call(
				  Expression.Convert(
					 instParamExpr,
					 info.DeclaringType
				  ),
				  info,
				  argsExtractionExpr
			   );

			var endLabel = Expression.Label(typeof(object));
			var finalExpr = info.ReturnType == typeof(void)
			   ? (Expression)Expression.Block(
					callExpr,
					Expression.Return(endLabel, Expression.Constant(null)),
					Expression.Label(endLabel, Expression.Constant(null))
				 )
			   : Expression.Convert(callExpr, typeof(object));

			var lambdaExpression = Expression.Lambda<Func<object, object[], object>>(
			   finalExpr,
			   instParamExpr,
			   argsParamExpr
			);
			var compiledLambda = lambdaExpression.Compile();

			_methodInvocationCache.AddOrUpdate(info, compiledLambda, (k, v) => compiledLambda);

			return compiledLambda;
		}

		private static ConcurrentDictionary<Type, PropertyInfo> _propInfoCache = new ConcurrentDictionary<Type, PropertyInfo>();

		public static async Task<object> ConvertAsync(this Task task)
		{
			await task.ConfigureAwait(false);

			var taskType = task.GetType();

			object result;
			if (_propInfoCache.TryGetValue(taskType, out var val))
			{
				result = val.GetValue(task);
			}
			else
			{
				var resultProp = taskType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
				_propInfoCache[taskType] = resultProp;

				result = resultProp.GetValue(task);
			}

			return result;
		}
	}
}
