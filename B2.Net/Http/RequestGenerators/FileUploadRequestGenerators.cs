using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using B2.Models;
using Newtonsoft.Json;

namespace B2.Http.RequestGenerators;

public static class FileUploadRequestGenerators {
	static class Endpoints {
		public const string GET_UPLOAD_URL = "b2_get_upload_url";
	}

	/// <summary>
	/// Upload a file to B2. This method will calculate the SHA1 checksum before sending any data.
	/// </summary>
	/// <param name="uploadUrl"></param>
	/// <param name="fileData"></param>
	/// <param name="fileName"></param>
	/// <param name="fileInfo"></param>
	/// <param name="contentType"></param>
	/// <returns></returns>
	public static HttpRequestMessage Upload(B2UploadUrl uploadUrl, byte[] fileData, string fileName, Dictionary<string, string>? fileInfo, string? contentType = null) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri(uploadUrl.UploadUrl),
			Content = new ByteArrayContent(fileData)
		};

		// Add headers
		request.Headers.TryAddWithoutValidation("Authorization", uploadUrl.AuthorizationToken);
		request.Headers.Add("X-Bz-File-Name", fileName.B2UrlEncode());
		request.Headers.Add("X-Bz-Content-Sha1", Utilities.GetSha1Hash(fileData));

		// File Info headers
		if (fileInfo is { Count: > 0 }) {
			foreach ((string key, string value) in fileInfo.Take(10)) {
				request.Headers.Add($"X-Bz-Info-{key}", value);
			}
		}

		request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType);
		request.Content.Headers.ContentLength = fileData.Length;

		return request;
	}

	/// <summary>
	/// Upload a file to B2 using a stream. NOTE: You MUST provide the SHA1 at the end of your stream. This method will NOT do it for you.
	/// </summary>
	/// <param name="uploadUrl"></param>
	/// <param name="fileDataWithSha"></param>
	/// <param name="fileName"></param>
	/// <param name="fileInfo"></param>
	/// <param name="contentType"></param>
	/// <param name="dontSha"></param>
	/// <returns></returns>
	public static HttpRequestMessage Upload(B2UploadUrl uploadUrl, Stream fileDataWithSha, string fileName, Dictionary<string, string>? fileInfo, string? contentType = null, bool dontSha = false) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri(uploadUrl.UploadUrl),
			Content = new StreamContent(fileDataWithSha)
		};

		// Add headers
		request.Headers.TryAddWithoutValidation("Authorization", uploadUrl.AuthorizationToken);
		request.Headers.Add("X-Bz-File-Name", fileName.B2UrlEncode());
		// Stream puts the SHA1 at the end of the content
		request.Headers.Add("X-Bz-Content-Sha1", dontSha ? "do_not_verify" : "hex_digits_at_end");

		// File Info headers
		if (fileInfo is { Count: > 0 }) {
			foreach (KeyValuePair<string, string> info in fileInfo.Take(10)) {
				request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
			}
		}

		request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType);
		// SHA will be in Stream already
		request.Content.Headers.ContentLength = fileDataWithSha.Length;

		return request;
	}

	public static HttpRequestMessage GetUploadUrl(B2Options options, string bucketId) {
		return BaseRequestGenerator.PostRequestJson(
			Endpoints.GET_UPLOAD_URL,
			JsonConvert.SerializeObject(new { bucketId }),
			options
		);
	}
}
