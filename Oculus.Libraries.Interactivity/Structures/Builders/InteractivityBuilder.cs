using Discord;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Libraries.Interactivity.Structures.Builders
{
    public class InteractivityBuilder
    {
        public readonly string Id;

        private List<SelectionContext> ButtonContexts { get; } = new();

        private List<MenuContext> MenuContexts { get; } = new();

        private PaginationContext? PaginationContext = null;


        private ComponentBuilder Components { get; } = new();


        public InteractivityBuilder()
        {
            Id = Guid.NewGuid().ToString();
        }


        public InteractivityBuilder AddMenu(MenuBuilder builder)
        {
            var menu = builder.Build(Id);
            MenuContexts.Add(menu);

            foreach (var option in menu.Options)
                menu.Menu.Options.Add(option.Component);

            Components.WithSelectMenu( menu.Menu.WithCustomId(Guid.NewGuid().ToString()) );

            return this;
        }

        public InteractivityBuilder AddSelection(SelectionBuilder builder)
        {
            var buttonRow = builder.Build(Id);
            ButtonContexts.Add(buttonRow);


            var row = new ActionRowBuilder();
            foreach (var button in buttonRow.Buttons)
                row.AddComponent( button.Component.WithUrl(null).WithCustomId(Guid.NewGuid().ToString()).Build() );

            Components.AddRow(row);

            return this;
        }

        public InteractivityBuilder WithPagination(PaginationBuilder builder)
        {
            var pagination = builder.Build(Id);
            PaginationContext = pagination;


            var row = new ActionRowBuilder();
            foreach (var button in pagination.Buttons)
                row.AddComponent( button.Component.WithUrl(null).WithCustomId(Guid.NewGuid().ToString()).Build() );

            Components.AddRow(row);

            return this;
        }

        public InteractivitySession Build()
        {
            return new InteractivitySession(Id, Components, ButtonContexts, MenuContexts, PaginationContext);
        }
    }
}