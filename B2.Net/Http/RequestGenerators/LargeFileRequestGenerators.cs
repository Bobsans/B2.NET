using B2.Models;

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
		HttpRequestMessage request = BaseRequestGenerator.PostRequestJson(Endpoints.START, new {
			bucketId,
			fileName,
			contentType = string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType
		}, options);

		// File Info headers
		if (fileInfo is { Count: > 0 }) {
			foreach ((string key, string value) in fileInfo.Take(10)) {
				request.Headers.Add($"X-Bz-Info-{key}", value);
			}
		}

		return request;
	}

	/// <summary>
	/// Upload a file to B2. This method will calculate the SHA1 checksum before sending any data.
	/// </summary>
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
		string hash = Utils.GetSha1Hash(fileData);

		// Add headers
		request.Headers.TryAddWithoutValidation("Authorization", uploadPartUrl.AuthorizationToken);
		request.Headers.Add("X-Bz-Part-Number", partNumber.ToString());
		request.Headers.Add("X-Bz-Content-Sha1", hash);
		request.Content.Headers.ContentLength = fileData.Length;

		return request;
	}

	public static HttpRequestMessage GetUploadPartUrl(B2Options options, string fileId) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.GET_PART_URL, new {
			fileId
		}, options);
	}

	public static HttpRequestMessage Finish(B2Options options, string fileId, string[] partSha1Array) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.FINISH, new {
			fileId,
			partSha1Array
		}, options);
	}

	public static HttpRequestMessage ListParts(B2Options options, string fileId, int startPartNumber, int maxPartCount) {
		if (startPartNumber is < 1 or > 10000) {
			throw new Exception("Start part number must be between 1 and 10,000");
		}

		return BaseRequestGenerator.PostRequestJson(Endpoints.LIST_PARTS, new {
			fileId,
			startPartNumber,
			maxPartCount
		}, options);
	}


	public static HttpRequestMessage Cancel(B2Options options, string fileId) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.CANCEL, new {
			fileId
		}, options);
	}

	public static HttpRequestMessage IncompleteFiles(B2Options options, string bucketId, string? startFileId = null, int? maxFileCount = null) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.INCOMPLETE_FILES, new {
			bucketId,
			startFileId,
			maxFileCount
		}, options);
	}

	public static HttpRequestMessage CopyPart(B2Options options, string sourceFileId, string largeFileId, int partNumber, string? range = null) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.COPY_PART, new {
			sourceFileId,
			largeFileId,
			partNumber,
			range
		}, options);
	}
}
