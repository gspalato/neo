using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Qmmands;

using Muon.Commands;

namespace Muon.Kernel.Structures.Attributes
{
	public class RequireOwnerAttribute : MuonCheckBase {
		public override async ValueTask<CheckResult> CheckAsync(MuonContext context, IServiceProvider provider) {
			RestApplication app = await context.Client.GetApplicationInfoAsync();

			if (app.Owner.Id == context.User.Id || context.Client.CurrentUser.Id == context.User.Id)
				return CheckResult.Successful;

			Console.WriteLine("failed xd");

			return CheckResult.Unsuccessful(
				Command is null
				? "No commands :P"
				: "You lack permissions to execute this command.");
		}
	}
}
