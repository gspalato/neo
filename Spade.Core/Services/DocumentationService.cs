using Spade.Core.Structures.Miscellaneous;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spade.Core.Services
{
	public interface IDocumentationService
	{
		Task<DocumentationApiResponse> GetDocumentationResultsAsync(string query);
	}

	public class DocumentationService : IDocumentationService
	{
		private const string ApiReferenceUrl = "https://docs.microsoft.com/api/apibrowser/dotnet/search?api-version=0.2&search=";
		private const string ApiFilter = "&locale=en-us&$filter=monikers/any(t:%20t%20eq%20%27netcore-3.0%27)%20or%20monikers/any(t:%20t%20eq%20%27netframework-4.8%27)";

		private readonly HttpClientHandler Handler;

		public DocumentationService()
		{
			Handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};
		}

		public async Task<DocumentationApiResponse> GetDocumentationResultsAsync(string query)
		{
			using var client = new HttpClient(Handler, false);
			var response = await client.GetAsync($"{ApiReferenceUrl}{query}{ApiFilter}");

			if (!response.IsSuccessStatusCode)
				throw new WebException("Something failed while querying the .NET API docs.");

			var jsonResponse = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<DocumentationApiResponse>(jsonResponse);
		}
	}
}
