using System.Collections.Generic;

namespace Spade.Core.Structures.Miscellaneous
{
    public record DocumentationApiResponse
    {
        public List<DocumentationMember> Results { get; init; } = new();

        public int Count { get; init; }

        public void Deconstruct(out List<DocumentationMember> members, out int count)
            => (members, count) = (Results, Count);
    }

    public record DocumentationMember
    {
        public string DisplayName { get; init; }

        public string Url { get; init; }

        public string ItemType { get; init; }

        public string ItemKind { get; init; }

        public string Description { get; init; }
    }
}
