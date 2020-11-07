using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spade.Core.Commands.TypeParsers
{
	public class TimeSpanTypeParser : BaseTypeParser<TimeSpan>
	{
		public static readonly TimeSpanTypeParser Instance = new TimeSpanTypeParser();

        private static readonly string[] Formats = {
            "%d'd'%h'h'%m'm'%s's'",
            "%d'd'%h'h'%m'm'",
            "%d'd'%h'h'%s's'",
            "%d'd'%h'h'",
            "%d'd'%m'm'%s's'",
            "%d'd'%m'm'",
            "%d'd'%s's'",
            "%d'd'",
            "%h'h'%m'm'%s's'",
            "%h'h'%m'm'",
            "%h'h'%s's'",
            "%h'h'",
            "%m'm'%s's'",
            "%m'm'",
            "%s's'",
            "%d'.'%h':'%m':'%s",
            "%h':'%m':'%s",
            "%m':'%s",
            "%s"
        };

        private TimeSpanTypeParser() { }

		public override async ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string value,
			SpadeContext context, IServiceProvider provider)
		{
            await Task.CompletedTask;

			return TimeSpan.TryParseExact(value.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, out var timeSpan)
				? new TypeParserResult<TimeSpan>(timeSpan)
				: new TypeParserResult<TimeSpan>("Unrecognized timespan.");
		}
	}
}