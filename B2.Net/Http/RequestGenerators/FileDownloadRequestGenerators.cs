using System;
using System.Net.Http;
using B2.Models;
using Newtonsoft.Json;

namespace B2.Http.RequestGenerators;

public static class FileDownloadRequestGenerators {
	static class Endpoints {
		public const string DOWNLOAD_BY_ID = "b2_download_file_by_id";
		public const string GET_DOWNLOAD_AUTHORIZATION = "b2_get_download_authorization";
		public const string DOWNLOAD_BY_NAME = "file";
	}

	public static HttpRequestMessage DownloadById(B2Options options, string fileId, string byteRange = "") {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.DownloadUrl}/b2api/{Constants.Version}/{Endpoints.DOWNLOAD_BY_ID}"),
			Content = new StringContent(JsonConvert.SerializeObject(new { fileId }))
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		// Add byte range header if we have it
		if (!string.IsNullOrEmpty(byteRange)) {
			request.Headers.Add("Range", $"bytes={byteRange}");
		}

		return request;
	}

	public static HttpRequestMessage DownloadByName(B2Options options, string bucketName, string fileName, string byteRange = "") {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Get,
			RequestUri = new Uri($"{options.DownloadUrl}/{Endpoints.DOWNLOAD_BY_NAME}/{bucketName}/{fileName.B2UrlEncode()}")
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		// Add byte range header if we have it
		if (!string.IsNullOrEmpty(byteRange)) {
			request.Headers.Add("Range", $"bytes={byteRange}");
		}

		return request;
	}

	public static HttpRequestMessage GetDownloadAuthorization(B2Options options, string fileNamePrefix, int validDurationInSeconds, string bucketId, string b2ContentDisposition = "") {
		string body = "{\"bucketId\":" + JsonConvert.ToString(bucketId) + ", \"fileNamePrefix\":" +
			JsonConvert.ToString(fileNamePrefix) + ", \"validDurationInSeconds\":" +
			JsonConvert.ToString(validDurationInSeconds);
		
		if (!string.IsNullOrEmpty(b2ContentDisposition)) {
			body += ", \"b2ContentDisposition\":" + JsonConvert.ToString(b2ContentDisposition);
		}

		body += "}";

		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.Version}/{Endpoints.GET_DOWNLOAD_AUTHORIZATION}"),
			Content = new StringContent(body)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		return request;
	}
}
