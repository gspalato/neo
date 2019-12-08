/* ActivatorUtilities.cs by trinitrotoluene */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Arpa.Utilities
{
	internal delegate T InstanceCreator<T>(params object[] ctorArgs);

	internal delegate object InstanceCreator(params object[] ctorArgs);

	internal static class ActivatorUtilities
	{
		private static readonly ConcurrentDictionary<Type, Delegate> TypedActivatorCache = new ConcurrentDictionary<Type, Delegate>();

		public static InstanceCreator<T> GetInstanceCreator<T>(ConstructorInfo ctorInfo)
		{
			if (TypedActivatorCache.TryGetValue(ctorInfo.DeclaringType, out var activator))
			{
				return (InstanceCreator<T>)activator;
			}

			var compiledLambda = CreateInstanceCreatorLambda<InstanceCreator<T>>(ctorInfo).Compile();

			TypedActivatorCache[ctorInfo.DeclaringType] = compiledLambda;

			return compiledLambda;
		}

		private static readonly ConcurrentDictionary<Type, InstanceCreator> ActivatorCache = new ConcurrentDictionary<Type, InstanceCreator>();


		public static InstanceCreator GetInstanceCreator(ConstructorInfo ctorInfo)
		{
			if (ActivatorCache.TryGetValue(ctorInfo.DeclaringType, out var activator))
			{
				return activator;
			}

			var compiledLambda = CreateInstanceCreatorLambda<InstanceCreator>(ctorInfo).Compile();

			ActivatorCache[ctorInfo.DeclaringType] = compiledLambda;

			return compiledLambda;
		}

		private static Expression<T> CreateInstanceCreatorLambda<T>(ConstructorInfo ctorInfo) where T : Delegate
		{
			var parameters = ctorInfo.GetParameters();

			var paramExpr = Expression.Parameter(typeof(object[]), "ctorArgs");

			var property = Expression.Property(paramExpr, "Length");
			var getCtorArgsLenExpr = Expression.Convert(property, typeof(int));

			Expression[] paramExprs = new Expression[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				var getParamAtIndexExpr = Expression.ArrayIndex(paramExpr, Expression.Constant(i));
				var castParamExpr = Expression.ConvertChecked(getParamAtIndexExpr, parameters[i].ParameterType);

				var returnBlockExpr = Expression.Block(
					Expression.Condition(
						Expression.AndAlso(Expression.GreaterThanOrEqual(
							Expression.Constant(i),
							getCtorArgsLenExpr),
							Expression.Constant(parameters[i].IsOptional)),
						(parameters[i].IsOptional ?
							Expression.Convert(Expression.Constant(parameters[i].DefaultValue), parameters[i].ParameterType)
							: Expression.Convert(Expression.Default(parameters[i].ParameterType), parameters[i].ParameterType)),
						castParamExpr)
				);

				paramExprs[i] = returnBlockExpr;
			}

			return Expression.Lambda<T>(Expression.New(ctorInfo, paramExprs), paramExpr);
		}
	}
}