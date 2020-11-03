using Qmmands;
using System.Collections.Generic;

namespace Spade.Core.Commands.ArgumentParsers
{
	public class UnixArgumentParserResult : ArgumentParserResult
	{
		internal static UnixArgumentParserResult UnknownParameter(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments, string parameterName, int position)
		{
			return new UnixArgumentParserResult(context, arguments, UnixArgumentParseFailure.UnknownParameter, $"An argument called \"{parameterName}\" was passed that is not a known parameter.", position);
		}

		internal static UnixArgumentParserResult UnexpectedQuote(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments, int position)
		{
			return new UnixArgumentParserResult(context, arguments, UnixArgumentParseFailure.UnexpectedQuote,
				"Unexpected quote.", position);
		}

		internal static UnixArgumentParserResult UnclosedQuote(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments, int position)
		{
			return new UnixArgumentParserResult(context, arguments, UnixArgumentParseFailure.UnclosedQuote,
				"Unclosed quote.", position);
		}

		internal static UnixArgumentParserResult TooFewArguments(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments, Parameter expectedParameter)
		{
			return new UnixArgumentParserResult(context, arguments, UnixArgumentParseFailure.TooFewArguments,
				$"A parameter of name \"{expectedParameter.Name}\" is missing.", null, expectedParameter);
		}

		internal static UnixArgumentParserResult Successful(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments)
		{
			return new UnixArgumentParserResult(context, arguments);
		}

		private UnixArgumentParserResult(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments, UnixArgumentParseFailure failure, string reason, int? position, Parameter parameter = null) : base(arguments)
		{
			_reason = reason;
			ParseFailure = failure;
			Position = position;
			RawArguments = Context.RawArguments;
			Expected = parameter;
			IsSuccessful = false;
			Context = context;
		}

		private UnixArgumentParserResult(CommandContext context, IReadOnlyDictionary<Parameter, object> arguments) : base(arguments)
		{
			Context = context;
			RawArguments = Context.RawArguments;
			IsSuccessful = true;
		}

		private readonly string _reason;

		public override bool IsSuccessful { get; }
		public CommandContext Context { get; }
		public string RawArguments { get; }
		public UnixArgumentParseFailure? ParseFailure { get; }
		public Parameter Expected { get; }
		public int? Position { get; }

		public override string Reason => _reason!;
	}

	public enum UnixArgumentParseFailure
	{
		UnknownParameter,
		UnexpectedQuote,
		UnclosedQuote,
		TooFewArguments
	}
}