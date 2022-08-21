using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace B2.Http;

public static class ResponseParser {
	public static async Task<T> ParseResponse<T>(HttpResponseMessage response, string callingApi = "") {
		string jsonResponse = await response.Content.ReadAsStringAsync();

		await Utilities.CheckForErrors(response, callingApi);

		return JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings {
			NullValueHandling = NullValueHandling.Ignore
		})!;
	}
}
