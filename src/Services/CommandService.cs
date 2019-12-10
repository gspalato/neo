using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord.WebSocket;

using Arpa.Errors;
using Arpa.Entities;
using Arpa.Structures;
using Arpa.Utilities;

/*
	CommandService

	> This class shall handle command storage and execution.
	> Execution also involves string parsing and argument resolving.

	Features:
	[?] Type resolvers;
	[X] Automatic module (command) discovery;
	[] Multi argument match handling;
	[] Command overloading;
*/

namespace Arpa.Services
{
	public class CommandService : ICommandService
	{
		private readonly DiscordSocketClient client;
		private readonly IServiceProvider services;
		private readonly CommandHandlerService handlerService;

		public readonly CommandMap commands;
		public readonly Dictionary<Type, object> typeParsers;

		public string prefix;

		public CommandService(
			DiscordSocketClient client,
			IServiceProvider services,
			CommandHandlerService handlerService
		)
		{
			this.services = services;
			this.client = client;
			this.handlerService = handlerService;

			this.commands = new CommandMap();
			this.typeParsers = new Dictionary<Type, object>();
		}

		public void AddCommand(CommandInfo info, Type cmd) =>
			commands.AddCommand(info.Id, cmd);

		public void AddModules()
		{
			foreach (Type t in Assembly.GetEntryAssembly().GetExportedTypes())
			{
				if (t.IsSubclassOf(typeof(Command)) && typeof(Command).IsAssignableFrom(t))
					HandleCommandLoading(t);

				Type typeParserInterface = t.GetInterfaces().FirstOrDefault(
					i => i.GetTypeInfo().IsGenericType
					&& i.GetGenericTypeDefinition() == typeof(ITypeParser<>)
				);

				if (typeParserInterface != null)
					HandleTypeParserLoading(t, typeParserInterface);
			}
		}

		public void AddTypeParser(Type type, dynamic parser)
		{
			if (this.typeParsers.TryGetValue(type, out object _))
				return;

			this.typeParsers.Add(type, parser);
		}

		public void Execute(CommandContext ctx)
		{
			string prefix = services.GetRequiredService<IConfiguration>()["Environment:DEV:PREFIX"];
			string content = ctx.Message.Content.Substring(prefix.Length);

			List<string> args = this.handlerService.ParseMessage(content);

			Command cmd = this.commands.GetCommand(args[0]);

			cmd.SetContext(ctx);
			cmd.GetType().GetMethod("RunAsync").Invoke(cmd,
				ParseRequiredArguments(cmd.GetType(), ctx, args).ToArray());
		}

		private object GetTypeParser(Type type) =>
			this.typeParsers.Where((type) => type.Equals(type)).FirstOrDefault();

		private void HandleCommandLoading(Type t)
		{
			try
			{
				CommandAttributes attributes = this.ReadCommandAttributes(t);
				CommandInfo info = new CommandInfo
				{
					Id = attributes.Id
				};

				this.AddCommand(info, t);
			}
			catch (Exception e)
			{
				if (e is NonCommandException)
					return;
			}
		}

		private void HandleTypeParserLoading(Type t, Type iT)
		{
			Type targetType = iT.GetGenericArguments().First();

			object parser = ClassUtilities.Instantiate(t);

			AddTypeParser(targetType, parser);
		}

		private List<object> ParseRequiredArguments(Type cmd, CommandContext ctx, List<string> args)
		{
			IEnumerable<ParameterInfo> infos = cmd
				.GetMethod("RunAsync")
				.GetParameters();

			List<dynamic> resolved = new List<dynamic>();

			int i = 1;
			foreach (ParameterInfo info in infos)
			{
				Type type = info.ParameterType.GetType();
				object parser = GetTypeParser(type);

				dynamic result = parser.GetType().GetMethod("ParseAsync").Invoke(parser, new object[] {
					args[i], ctx, i
				});

				Console.WriteLine(type.ToString(), parser.ToString(), result);

				resolved.Add(result);

				i++;
			}

			return resolved;
		}

		private CommandAttributes ReadCommandAttributes(Type type)
		{
			CommandAttribute cmdAttribute = type.GetCustomAttribute<CommandAttribute>();
			if (cmdAttribute == null)
				throw new NonCommandException();

			CommandAttributes attributes = new CommandAttributes(cmdAttribute.Id);
			return attributes;
		}
	}
}