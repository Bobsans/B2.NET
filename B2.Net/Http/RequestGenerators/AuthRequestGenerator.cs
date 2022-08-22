using B2.Models;

namespace B2.Http.RequestGenerators;

public static class AuthRequestGenerator {
	static class Endpoints {
		public const string AUTH = "b2_authorize_account";
	}

	public static HttpRequestMessage Authorize(B2Options options) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Get,
			RequestUri = new Uri($"{Constants.API_BASE_URL}/{Constants.VERSION}/{Endpoints.AUTH}")
		};

		request.Headers.Add("Authorization", Utils.CreateAuthorizationHeader(options.KeyId, options.ApplicationKey));

		return request;
	}
}
