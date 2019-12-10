using System;
using System.Threading.Tasks;

using Arpa.Structures;

namespace Arpa.Entities
{
	public interface ITypeParser { }

	public interface ITypeParser<T>
	{
		Task<T> ParseAsync(string arg, CommandContext ctx, int position);
	}
}