namespace B2.Http;

public static class ResponseParser {
	public static async Task<T> ParseResponse<T>(HttpResponseMessage response, string? callingApi = null) where T : new() {
		await Utils.CheckForErrors(response, callingApi);
		return Utils.Deserialize<T>(await response.Content.ReadAsStringAsync())!;
	}
}
