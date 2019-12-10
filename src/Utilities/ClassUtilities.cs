using System;
using System.Reflection;

namespace Arpa.Utilities
{
	public class ClassUtilities
	{
		public static Type GetBaseType(Type baseClass, Type type)
		{
			while (type.BaseType != null)
			{
				type = type.BaseType;
				if (type.IsGenericType && type.GetGenericTypeDefinition() == baseClass)
				{
					return type.GetGenericArguments()[0];
				}
			}
			throw new InvalidOperationException("Base type was not found");
		}

		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
				{
					return true;
				}
				toCheck = toCheck.GetTypeInfo().BaseType;
			}
			return false;
		}

		public static T Instantiate<T>(Type t) where T : class =>
			Utilities.ActivatorUtilities.GetInstanceCreator(t.GetConstructor(Type.EmptyTypes))() as T;

		public static object Instantiate(Type t) =>
			Utilities.ActivatorUtilities.GetInstanceCreator(t.GetConstructor(Type.EmptyTypes))();
	}
}