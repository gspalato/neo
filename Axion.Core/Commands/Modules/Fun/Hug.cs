using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Fun
{
    [Category(Category.Fun)]
    [Description("Hug someone! **(っ´▽`)っ**")]
    [Group("hug")]
    public class Hug : AxionModule
    {
        public Random Random { get; set; }

        [Command]
        [IgnoresExtraArguments]
        public async Task ExecuteAsync(IGuildUser user)
        {
            Console.WriteLine(user);

            string who;
            if (user.Id == Context.User.Id)
                who = "themself";
            else if (user.Id == Context.Client.CurrentUser.Id)
                who = "me";
            else
                who = user.Mention;

            var image = GetRandomHugGif();
            var embed = new EmbedBuilder()
                .WithDescription($"{Context.User.Mention} hugged {who}! **(っ´▽`)っ**")
                .WithInfo()
                .WithImageUrl(image);

            await SendEmbedAsync(embed);
        }

        private string GetRandomHugGif()
        {
            var links = new string[] {
                "https://media1.tenor.com/images/7db5f172665f5a64c1a5ebe0fd4cfec8/tenor.gif?itemid=9200935",
                "https://media1.tenor.com/images/42922e87b3ec288b11f59ba7f3cc6393/tenor.gif?itemid=5634630",
                "https://media1.tenor.com/images/b4ba20e6cb49d8f8bae81d86e45e4dcc/tenor.gif?itemid=5634582",
                "https://media.giphy.com/media/Lp6T9KxDEgsWA/giphy.gif",
                "https://media1.tenor.com/images/b0de026a12e20137a654b5e2e65e2aed/tenor.gif?itemid=7552093",
                "https://media.giphy.com/media/wnsgren9NtITS/giphy.gif",
                "http://37.media.tumblr.com/66c19998360481a17ca928283006297c/tumblr_n4i4jvTWLe1sg0ygjo1_500.gif",
                "https://i.imgur.com/anqcRxv.gif"
            };

            int index;
            lock (Random)
            {
                index = Random.Next(1, links.Length + 1);
            }

            return links[index];
        }
    }
}
