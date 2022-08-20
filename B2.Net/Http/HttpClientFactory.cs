using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace B2.Http; 

public static class HttpClientFactory {
	static HttpClient _client;

	public static HttpClient CreateHttpClient(int timeout) {
		HttpClient client = _client;
		if (client == null) {
			HttpClientHandler handler = new() { AllowAutoRedirect = true };

			client = new HttpClient(handler);

			client.Timeout = TimeSpan.FromSeconds(timeout);

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client = client;
		}
		return client;
	}
}
