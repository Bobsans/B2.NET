﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using B2.Models;

namespace B2.Http.RequestGenerators;

public static class BaseRequestGenerator {
	public static HttpRequestMessage PostRequest(string endpoint, string body, B2Options options) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.Version}/{endpoint}"),
			Content = new StringContent(body)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		return request;
	}

	public static HttpRequestMessage PostRequestJson(string endpoint, string body, B2Options options) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.Version}/{endpoint}"),
			Content = new StringContent(body)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		request.Content.Headers.ContentLength = body.Length;

		return request;
	}
}
