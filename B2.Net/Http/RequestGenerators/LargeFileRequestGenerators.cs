using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using B2.Models;
using Newtonsoft.Json;

namespace B2.Http.RequestGenerators;

public static class LargeFileRequestGenerators {
	static class Endpoints {
		public const string START = "b2_start_large_file";
		public const string GET_PART_URL = "b2_get_upload_part_url";
		public const string UPLOAD = "b2_upload_part";
		public const string FINISH = "b2_finish_large_file";
		public const string LIST_PARTS = "b2_list_parts";
		public const string CANCEL = "b2_cancel_large_file";
		public const string INCOMPLETE_FILES = "b2_list_unfinished_large_files";
		public const string COPY_PART = "b2_copy_part";
	}

	public static HttpRequestMessage Start(B2Options options, string bucketId, string fileName, string? contentType = null, Dictionary<string, string>? fileInfo = null) {
		string content = $"{{\"bucketId\":\"{bucketId}\",\"fileName\":\"{fileName}\",\"contentType\":\"{(string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType)}\"}}";

		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.Version}/{Endpoints.START}"),
			Content = new StringContent(content)
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		// File Info headers
		if (fileInfo is { Count: > 0 }) {
			foreach ((string key, string value) in fileInfo.Take(10)) {
				request.Headers.Add($"X-Bz-Info-{key}", value);
			}
		}

		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		request.Content.Headers.ContentLength = content.Length;

		return request;
	}

	/// <summary>
	/// Upload a file to B2. This method will calculate the SHA1 checksum before sending any data.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="fileData"></param>
	/// <param name="partNumber"></param>
	/// <param name="uploadPartUrl"></param>
	/// <returns></returns>
	public static HttpRequestMessage Upload(B2Options options, byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl) {
		if (partNumber is < 1 or > 10000) {
			throw new Exception("Part number must be between 1 and 10,000");
		}

		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri(uploadPartUrl.UploadUrl),
			Content = new ByteArrayContent(fileData)
		};

		// Get the file checksum
		string hash = Utilities.GetSha1Hash(fileData);

		// Add headers
		request.Headers.TryAddWithoutValidation("Authorization", uploadPartUrl.AuthorizationToken);
		request.Headers.Add("X-Bz-Part-Number", partNumber.ToString());
		request.Headers.Add("X-Bz-Content-Sha1", hash);
		request.Content.Headers.ContentLength = fileData.Length;

		return request;
	}

	public static HttpRequestMessage GetUploadPartUrl(B2Options options, string fileId) {
		return BaseRequestGenerator.PostRequest(Endpoints.GET_PART_URL, JsonConvert.SerializeObject(new { fileId }), options);
	}

	public static HttpRequestMessage Finish(B2Options options, string fileId, string[] partSha1Array) {
		string content = JsonConvert.SerializeObject(new { fileId, partSha1Array });
		HttpRequestMessage request = BaseRequestGenerator.PostRequestJson(Endpoints.FINISH, content, options);
		return request;
	}

	public static HttpRequestMessage ListParts(B2Options options, string fileId, int startPartNumber, int maxPartCount) {
		if (startPartNumber is < 1 or > 10000) {
			throw new Exception("Start part number must be between 1 and 10,000");
		}

		return BaseRequestGenerator.PostRequestJson(
			Endpoints.LIST_PARTS,
			JsonConvert.SerializeObject(new { fileId, startPartNumber, maxPartCount }),
			options
		);
	}

	public static HttpRequestMessage Cancel(B2Options options, string fileId) {
		return BaseRequestGenerator.PostRequestJson(
			Endpoints.CANCEL,
			JsonConvert.SerializeObject(new { fileId }),
			options
		);
	}

	public static HttpRequestMessage IncompleteFiles(B2Options options, string bucketId, string startFileId = "", string maxFileCount = "") {
		string body = "{\"bucketId\":\"" + bucketId + "\"";

		if (!string.IsNullOrEmpty(startFileId)) {
			body += ", \"startFileId\":" + JsonConvert.ToString(startFileId);
		}

		if (!string.IsNullOrEmpty(maxFileCount)) {
			body += ", \"maxFileCount\":" + JsonConvert.ToString(maxFileCount);
		}

		body += "}";

		return BaseRequestGenerator.PostRequestJson(Endpoints.INCOMPLETE_FILES, body, options);
	}

	public static HttpRequestMessage CopyPart(B2Options options, string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string range = "") {
		Dictionary<string, string> payload = new() {
			{ "sourceFileId", sourceFileId },
			{ "largeFileId", destinationLargeFileId },
			{ "partNumber", destinationPartNumber.ToString() },
		};

		if (!string.IsNullOrWhiteSpace(range)) {
			payload.Add("range", range);
		}

		string content = JsonConvert.SerializeObject(payload);

		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{options.ApiUrl}/b2api/{Constants.Version}/{Endpoints.COPY_PART}"),
			Content = new StringContent(content),
		};

		request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		request.Content.Headers.ContentLength = content.Length;

		return request;
	}
}
