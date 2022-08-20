using System;
using System.Net.Http;
using B2.Models;

namespace B2.Http.RequestGenerators; 

public static class AuthRequestGenerator {
	static class Endpoints {
		public const string AUTH = "b2_authorize_account";
	}

	public static HttpRequestMessage Authorize(B2Options options) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Get,
			RequestUri = new Uri($"{Constants.ApiBaseUrl}/{Constants.Version}/{Endpoints.AUTH}")
		};

		request.Headers.Add("Authorization", Utilities.CreateAuthorizationHeader(options.KeyId, options.ApplicationKey));

		return request;
	}
}
