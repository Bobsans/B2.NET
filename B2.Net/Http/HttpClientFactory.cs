using System.Net.Http.Headers;

namespace B2.Http;

public static class HttpClientFactory {
	static HttpClient? client;

	public static HttpClient CreateHttpClient(int timeout) {
		if (client == null) {
			client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true }) {
				Timeout = TimeSpan.FromSeconds(timeout)
			};

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		return client;
	}
}
