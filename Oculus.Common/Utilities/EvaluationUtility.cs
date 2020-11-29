using Oculus.Common.Extensions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Oculus.Common.Utilities
{
	public static class EvaluationUtility
	{
		public static string InspectInheritance(object obj) => Inheritance(obj.GetType());

		public static string Inheritance<T>() => Inheritance(typeof(T));

		public static string Inheritance(Type type)
		{
			var baseTypes = new List<Type>() { type };
			var latestType = type.BaseType;

			while (latestType is not null)
			{
				baseTypes.Add(latestType);
				latestType = latestType.BaseType;
			}

			var sb = new StringBuilder().AppendLine($"Inheritance graph for type [{type.FullName}]").AppendLine();

			foreach (var baseType in baseTypes)
			{
				sb.Append($"[{FormatType(baseType)}]");
				IList<Type> inheritors = baseType.GetInterfaces();
				if (baseType.BaseType is not null)
				{
					inheritors = inheritors.ToList();
					inheritors.Add(baseType.BaseType);
				}
				if (inheritors.Any()) sb.Append($": {string.Join(", ", inheritors.Select(b => b.FullName))}");

				sb.AppendLine();
			}

			return Format.Code(sb.ToString(), "ini");
		}


		private static string FormatType(Type atype)
		{
			var vs = new StringBuilder($"{atype.Namespace}.{atype.Name}");

			var t = atype.GenericTypeArguments;

			if (t.Length > 0)
				vs.Append($"<{string.Join(", ", t.Select(a => a.Name))}>");

			return vs.ToString();
		}
		
		public static string SerializeObject(object obj, bool serializeInner = true)
		{
			var type = obj.GetType();

			if (obj is string)
				return $"\"{obj.ToString()?.Replace("\n", @"\n")}\"";
			else if (obj is decimal)
				return obj.ToString();
			else if (type.IsPrimitive)
				return obj.ToString();

			static string ReplaceIndex(string s)
			{
				var indexed = Regex.Match(s, @"(?<=([a-zA-Z]+)`)([0-9]+)");
				if (indexed.Success)
					return s.ReplaceAt(indexed.Index - 1, indexed.Length + 1, $"[{indexed.Value}]");
				else
					return s;
			}

			if (!serializeInner)
				return $"[{ReplaceIndex(type.Name)}]";

			var props = type.GetProperties();

			var builder = new StringBuilder();
			builder.AppendLine($"{type.Name} {{");

			int total = 0;
			foreach (var prop in props)
			{
				if (total >= 10)
				{
					builder.AppendLine("\t...");
					break;
				}

				var value = prop.GetValue(obj);
				string serialized;
				if (value is not null)
					serialized = SerializeObject(value, false);
				else
					serialized = null;

				string typeName = ReplaceIndex(prop.PropertyType.Name);

				builder.Append($"\t{typeName} {prop.Name}");
				builder.Append($": {serialized ?? "null"}\n");

				total++;
			}

			builder.Append("}");

			return builder.ToString();
		}
	}
}