using Discord;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Libraries.Interactivity.Structures
{
    public class InteractivitySession
    {
        public readonly string Id;

        public List<SelectionContext> ButtonContexts { get; private set; } = new();

        public List<MenuContext> MenuContexts { get; private set; } = new();

        public PaginationContext? PaginationContext { get; private set; }


        internal ComponentBuilder Components { get; } = new();


        internal InteractivitySession(string id, ComponentBuilder components, List<SelectionContext> buttonContexts,
            List<MenuContext> menuContexts, PaginationContext? paginationContext = null)
        {
            Id = id;

            Components = components;
            ButtonContexts = buttonContexts;
            MenuContexts = menuContexts;
            PaginationContext = paginationContext;
        }
    }
}