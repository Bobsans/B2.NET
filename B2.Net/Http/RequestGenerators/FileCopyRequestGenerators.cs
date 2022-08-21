using System;
using System.Collections.Generic;
using System.Net.Http;
using B2.Models;
using Newtonsoft.Json;

namespace B2.Http.RequestGenerators;

public static class FileCopyRequestGenerators {
	static class Endpoints {
		public const string COPY = "b2_copy_file";
	}

	public static HttpRequestMessage Copy(
		B2Options options,
		string sourceFileId,
		string fileName,
		B2MetadataDirective metadataDirective,
		string contentType = "",
		Dictionary<string, string>? fileInfo = null,
		string range = "",
		string destinationBucketId = ""
	) {
		Dictionary<string, object> payload = new() {
			{ "sourceFileId", sourceFileId },
			{ "fileName", fileName },
			{ "metadataDirective", metadataDirective.ToString() },
		};

		if (!string.IsNullOrWhiteSpace(range)) {
			payload.Add("range", range);
		}

		if (!string.IsNullOrWhiteSpace(destinationBucketId)) {
			payload.Add("destinationBucketId", destinationBucketId);
		}

		if (metadataDirective == B2MetadataDirective.REPLACE) {
			payload.Add("contentType", string.IsNullOrWhiteSpace(contentType) ? "b2/x-auto" : contentType);
		}

		// File Info
		if (metadataDirective == B2MetadataDirective.REPLACE && fileInfo is { Count: > 0 }) {
			payload.Add("fileInfo", fileInfo);
		}

		string json = JsonConvert.SerializeObject(payload);

		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.Version}/{Endpoints.COPY}"),
			Content = new StringContent(json)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		return request;
	}
}
