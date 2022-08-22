using B2.Models;

namespace B2.Http.RequestGenerators;

public static class FileDownloadRequestGenerators {
	static class Endpoints {
		public const string DOWNLOAD_BY_ID = "b2_download_file_by_id";
		public const string GET_DOWNLOAD_AUTHORIZATION = "b2_get_download_authorization";
		public const string DOWNLOAD_BY_NAME = "file";
	}

	public static HttpRequestMessage DownloadById(B2Options options, string fileId, string? byteRange = null) {
		HttpRequestMessage request = BaseRequestGenerator.PostRequestJson(Endpoints.DOWNLOAD_BY_ID, new {
			fileId
		}, options);

		// Add byte range header if we have it
		if (!string.IsNullOrEmpty(byteRange)) {
			request.Headers.Add("Range", $"bytes={byteRange}");
		}

		return request;
	}

	public static HttpRequestMessage DownloadByName(B2Options options, string bucketName, string fileName, string? byteRange = null) {
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

	public static HttpRequestMessage GetDownloadAuthorization(B2Options options, string fileNamePrefix, int validDurationInSeconds, string bucketId, string? b2ContentDisposition = null) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.GET_DOWNLOAD_AUTHORIZATION, new {
			bucketId,
			fileNamePrefix,
			validDurationInSeconds,
			b2ContentDisposition
		}, options);
	}
}
