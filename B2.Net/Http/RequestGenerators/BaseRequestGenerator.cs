using System.Net.Http.Headers;
using B2.Models;

namespace B2.Http.RequestGenerators;

public static class BaseRequestGenerator {
	public static HttpRequestMessage PostRequest(string endpoint, string body, B2Options options) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.VERSION}/{endpoint}"),
			Content = new StringContent(body)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		return request;
	}

	public static HttpRequestMessage PostRequestJson<T>(string endpoint, T payload, B2Options options) {
		string content = Utils.Serialize(payload);

		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.VERSION}/{endpoint}"),
			Content = new StringContent(content)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		request.Content.Headers.ContentLength = content.Length;

		return request;
	}
}
