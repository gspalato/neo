using Axion.Core.Extensions;
using Axion.Core.Services;
using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Interactivity;
using Qmmands;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Description("Search for .NET Core documentation.")]
	[Group("docs")]
	public sealed class Documentation : AxionModule
	{
		public IDocumentationService DocumentationService { get; set; }

		[Command]
		public async Task ExecuteAsync([Name("Query"), Remainder, Description("The term to search for in the documentation.")] string query)
		{
			var reg = new Regex("^[0-9A-Za-z.<>]$");
			foreach (var c in query)
				if (!reg.IsMatch(c.ToString()))
				{
					var s = c == '\\' ? @"\\" : c.ToString();
					await Context.ReplyAsync($" '{s}' character is not allowed in the search, please try again.");
					return;
				}

			var response = await DocumentationService.GetDocumentationResultsAsync(query);

			if (response.Count == 0)
			{
				await Context.ReplyAsync("Could not find documentation for your requested query.");
				return;
			}

			var chunks = response.Results.Chunk(3).Take(5);
			var chunkCount = chunks.Count();

			var pagedBuilder = new PaginatedMessageBuilder()
				.WithDefaultButtons()
				.WithResponsible(Context.User);

			var totalMatchCount = 0;
			for (var chunkNumber = 0; chunkNumber < chunkCount; chunkNumber++)
			{
				var chunk = chunks.ToArray()[chunkNumber];
				var description = new StringBuilder();

				if (chunk is null)
					break;

				for (var matchNumber = 0; matchNumber < chunk.Count(); matchNumber++)
				{
					var res = chunk.ElementAt(matchNumber);

					totalMatchCount++;

					description.AppendLine($"\u2794 **[{res.ItemKind}: {res.DisplayName}]({res.Url})**");
					description.AppendLine($"{res.Description}");
					description.AppendLine();
				}

				if (chunkNumber == chunkCount - 1)
				{
					description.Append($"{totalMatchCount} of {response.Results.Count} results shown · ");
					description.Append("[click here for more results](https://docs.microsoft.com/dotnet/api/?term={query})");
				}

				var n = chunkNumber + 1;
				pagedBuilder.AddPage(template =>
				{
					template
						.WithTitle($"📜 dotnet documentation · page {n} of {chunkCount}")
						.WithDefaultColor()
						.WithDescription(description.ToString());
				});
			}

			await pagedBuilder.Build(Context.Client).Send(Context.Channel);
		}
	}
}