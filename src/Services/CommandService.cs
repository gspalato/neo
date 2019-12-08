using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord.WebSocket;

using Arpa.Errors;
using Arpa.Entities;
using Arpa.Structures;

/*
	CommandService

	> This class shall handle command storage and execution.
	> Execution also involves string parsing and argument resolving.

	Features:
	[]  Type resolvers;
	[X] Automatic module (command) discovery;
	[]  Multi argument match handling;
	[]  Command overloading;
*/

namespace Arpa.Services
{
	public interface _ICommandService
	{

	}

	public class _CommandService : _ICommandService
	{
		private readonly DiscordSocketClient client;
		private readonly IServiceProvider services;
		private readonly CommandHandlerService handlerService;

		public readonly _CommandMap commands;
		public readonly Map<Type, object> typeParsers;

		public string prefix;

		public _CommandService(
			DiscordSocketClient client,
			IServiceProvider services,
			CommandHandlerService handlerService
		)
		{
			this.services = services;
			this.client = client;
			this.handlerService = handlerService;

			this.commands = new _CommandMap();
			this.typeParsers = new Map<Type, object>();
		}

		public void AddCommand(_CommandInfo info, _Command cmd)
		{
			commands.AddCommand(info.Id, cmd);
		}

		public void AddModules()
		{
			foreach (Type t in Assembly.GetEntryAssembly().GetExportedTypes())
				if (t.IsSubclassOf(typeof(_Command)) && typeof(_Command).IsAssignableFrom(t))
				{
					_Command cmd = this.Instantiate<_Command>(t);
					try
					{
						_CommandAttributes attributes = this.ReadCommandAttributes(t);
						_CommandInfo info = new _CommandInfo
						{
							Id = attributes.Id
						};

						this.AddCommand(info, cmd);
					}
					catch (Exception e)
					{
						if (e is NonCommandException)
							continue;
					}
				}
				else if (t.IsSubclassOf(typeof(TypeParser<>)))
				{
					Type type = t.BaseType.GetGenericArguments()[0];
					dynamic parser = Convert.ChangeType(this.Instantiate(t), t.BaseType.MakeGenericType(type));
					Console.WriteLine(type.ToString(), parser);

					this.AddTypeParser(type, parser);
				}
		}

		public void AddTypeParser(Type type, dynamic parser)
		{
			this.typeParsers.Add(type, parser);
		}

		public void Execute(_CommandContext ctx)
		{
			string prefix = services.GetRequiredService<IConfiguration>()["Environment:PROD:PREFIX"];
			List<string> args = this.handlerService
				.ParseMessage(ctx.Message.Content.Substring(prefix.Length));

			_Command cmd = this.commands.FindCommands(
				(info) => info.Id == args[0]
			).First();

			cmd.SetContext(ctx);

			cmd.GetType().GetMethod("RunAsync").Invoke(
				cmd,
				ParseRequiredArguments(cmd.GetType(), ctx, args).ToArray()
			);
		}

		private List<dynamic> ParseRequiredArguments(Type cmd, _CommandContext ctx, List<string> args)
		{
			IEnumerable<ParameterInfo> infos = cmd
				.GetMethod("RunAsync")
				.GetParameters();

			List<dynamic> resolved = new List<dynamic>();

			int i = 1;
			foreach (ParameterInfo info in infos)
			{
				Type type = info.ParameterType.GetType();
				dynamic parser = GetTypeParser(type);

				dynamic result = parser.GetMethod("ParseAsync").Invoke(parser, new object[] {
					args[i],
					ctx,
					i
				}).Value;

				resolved.Add(result);

				i++;
			}

			return resolved;
		}

		private dynamic GetTypeParser(Type type)
		{
			return this.typeParsers.Find((t) => t.Equals(type)).First();
		}

		private T Instantiate<T>(Type t) where T : class
		{
			return Utilities.ActivatorUtilities.GetInstanceCreator(t.GetConstructor(Type.EmptyTypes))() as T;
		}
		private object Instantiate(Type t)
		{
			return Utilities.ActivatorUtilities.GetInstanceCreator(t.GetConstructor(Type.EmptyTypes))();
		}

		private _CommandAttributes ReadCommandAttributes(Type type)
		{
			_CommandAttribute cmdAttribute = type.GetCustomAttribute<_CommandAttribute>();
			if (cmdAttribute == null)
				throw new NonCommandException();

			_CommandAttributes attributes = new _CommandAttributes(cmdAttribute.Id);
			return attributes;
		}
	}
}