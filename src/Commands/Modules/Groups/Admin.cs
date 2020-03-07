using System;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon;
using Muon.Core.Structures;

namespace Muon.Commands
{
	[Category("Administration")]
	[Description("Elite-only commands.")]
	[Hidden]
	public partial class Admin : BaseCommandModule { }
}