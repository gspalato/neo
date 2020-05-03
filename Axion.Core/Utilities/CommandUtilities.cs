using System;
using System.Text;
using Victoria;
using Victoria.Interfaces;

namespace Axion.Core.Utilities
{
	public static class CommandUtilities
	{
		public static string GetNearestTracksAsString(DefaultQueue<IQueueable> queue)
		{
			if (queue.Count == 0)
				return "";

			StringBuilder s = new StringBuilder();

			int elapsed = 0;
			foreach (LavaTrack track in queue.Items)
			{
				if (elapsed >= 5)
					break;

				s.Append($"{++elapsed}. [{track.Title.TruncateAndSanitize()}]({track.Url})");
				s.Append("\n");
			}

			int remaining = queue.Count - elapsed;
			if (queue.Count > 5)
				s.Append($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return s.ToString();
		}

		public static string GetHugGif(Random random)
		{
			string[] links = new string[] {
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
			lock (random)
			{
				index = random.Next(1, links.Length + 1);
			}
			return links[index];
		}
	}
}
