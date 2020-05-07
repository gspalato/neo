using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Axion.Core.Commands;
using Axion.Core.Extensions;
using Discord;

namespace Axion.Core.Utilities
{
	public class EvaluationUtility
	{
        public AxionContext Context { get; }

        public EvaluationUtility(AxionContext context)
        {
            Context = context;
        }

        public static string Methods(object obj)
        {
            var type = obj as Type ?? obj.GetType();

            var sb = new StringBuilder();

            var methods = type.GetMethods();

            sb.Append($"<< Inspecting methods for type [{type.Name}] >>");
            sb.AppendLine();

            foreach (var method in methods.Where(m => !m.IsSpecialName))
            {
                if (sb.Length > 1800) break;
                sb.Append(
                    $"[Name: {method.Name}, Return-Type: {method.ReturnType.Name}, Parameters: [{string.Join(", ", method.GetParameters().Select(a => $"({a.ParameterType.Name} {a.Name})"))}]");
                sb.AppendLine();
            }

            return Format.Code(sb.ToString(), "ini");
        }


        public static string InspectInheritance(object obj) => Inheritance(obj.GetType());

        public static string Inheritance<T>() => Inheritance(typeof(T));

        public static string Inheritance(Type type)
        {
            var baseTypes = new List<Type>() { type };
            var latestType = type.BaseType;

            while (latestType != null)
            {
                baseTypes.Add(latestType);
                latestType = latestType.BaseType;
            }

            var sb = new StringBuilder().AppendLine($"Inheritance graph for type [{type.FullName}]").AppendLine();

            foreach (var baseType in baseTypes)
            {
                sb.Append($"[{FormatType(baseType)}]");
                IList<Type> inheritors = baseType.GetInterfaces();
                if (baseType.BaseType != null)
                {
                    inheritors = inheritors.ToList();
                    inheritors.Add(baseType.BaseType);
                }
                if (inheritors.Any()) sb.Append($": {string.Join(", ", inheritors.Select(b => b.FullName))}");

                sb.AppendLine();
            }

            return Format.Code(sb.ToString(), "ini");
        }

        public static string Inspect(object obj)
        {
            var type = obj.GetType();

            var inspection = new StringBuilder();
            inspection.Append("<< Inspecting type [").Append(type.Name).AppendLine("] >>");
            inspection.Append("<< String Representation: [").Append(obj).AppendLine("] >>");
            inspection.AppendLine();

            /* Get list of properties, with no index parameters (to avoid exceptions) */
            var props = type.GetProperties().Where(a => a.GetIndexParameters().Length == 0)
                .OrderBy(a => a.Name).ToList();

            /* Get list of fields */
            var fields = type.GetFields().OrderBy(a => a.Name).ToList();

            /* Handle properties in type */
            if (props.Count != 0)
            {
                /* Add header if we have fields as well */
                if (fields.Count != 0) inspection.AppendLine("<< Properties >>");

                /* Get the longest named property in the list, so we can make the column width that + 5 */
                var columnWidth = props.Max(a => a.Name.Length) + 5;
                foreach (var prop in props)
                {
                    /* Crude skip to avoid request errors */
                    if (inspection.Length > 1800) break;

                    /* Create a blank string gap of the remaining space to the end of the column */
                    var sep = new string(' ', columnWidth - prop.Name.Length);

                    /* Add the property name, then the separator, then the value */
                    inspection.Append(prop.Name).Append(sep).Append(prop.CanRead ? prop.GetValue(obj) : "Unreadable").AppendLine();
                }
            }

            /* Repeat the same with fields */
            if (fields.Count != 0)
            {
                if (props.Count != 0)
                {
                    inspection.AppendLine();
                    inspection.AppendLine("<< Fields >>");
                }

                var columnWidth = fields.Max(ab => ab.Name.Length) + 5;
                foreach (var prop in fields)
                {
                    if (inspection.Length > 1800) break;

                    var sep = new string(' ', columnWidth - prop.Name.Length);
                    inspection.Append(prop.Name).Append(":").Append(sep).Append(prop.GetValue(obj)).AppendLine();
                }
            }

            /* If the object is an enumerable type, add a list of it's items */
            // ReSharper disable once InvertIf
            if (obj is IEnumerable objEnumerable)
            {
                inspection.AppendLine();
                inspection.AppendLine("<< Items >>");
                foreach (var prop in objEnumerable) inspection.Append(" - ").Append(prop).AppendLine();
            }

            return Format.Code(inspection.ToString(), "ini");
        }


        private static string FormatType(Type atype)
        {
            var vs = atype.Namespace + "." + atype.Name;

            var t = atype.GenericTypeArguments;

            if (t.Length > 0) vs += $"<{string.Join(", ", t.Select(a => a.Name))}>";

            return vs;
        }

        public Task<IGuildUser> User(ulong id)
        {
            return Context.Guild.GetUserAsync(id);
        }

        public async Task<IGuildUser> User(string username)
        {
            var users = await Context.Guild.GetUsersAsync();
            return users.FirstOrDefault(a => 
                a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                || (a.Nickname != null && a.Nickname.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<ITextChannel> TextChannel(ulong id)
        {
            return Context.Guild.GetTextChannelAsync(id);
        }

        public async Task<ITextChannel> TextChannel(string name)
        {
            var channels = await Context.Guild.GetChannelsAsync();
            return channels.FirstOrDefault(a => 
                a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                && a is ITextChannel) as ITextChannel;
        }

        public Task<IMessage> Message(ulong id)
        {
            return Context.Channel.GetMessageAsync(id);
        }

        public Task<IMessage> Message(string id) => Message(ulong.Parse(id));


        public static string SerializeObject(object obj, bool serializeInner = true)
		{
			var type = obj.GetType();

			if (obj is string)
				return $"\"{obj.ToString().Replace("\n", @"\n")}\"";
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
				if (value != null)
					serialized = SerializeObject(value, false);
				else
					serialized = null;

				string typeName = ReplaceIndex(prop.PropertyType.Name);

				builder.Append($"\t<{typeName}> {prop.Name}");
				builder.Append($": {serialized ?? "null"}\n");

				total++;
			}

			builder.Append("}");

			return builder.ToString();
		}
	}
}