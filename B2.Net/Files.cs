using System.Net;
using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;

namespace B2;

public class Files : IFiles {
	const string API = "Files";

	readonly B2Options _options;
	readonly HttpClient _client;

	public Files(B2Options options) {
		_options = options;
		_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
	}

	/// <summary>
	/// Lists the names of all non-hidden files in a bucket, starting at a given name.
	/// </summary>
	public async Task<B2FileList> GetList(string? startFileName = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default) {
		return await GetListWithPrefixOrDelimiter(startFileName, null, null, maxFileCount, bucketId, cancelToken);
	}

	/// <summary>
	/// BETA: Lists the names of all non-hidden files in a bucket, starting at a given name. With an optional file prefix or delimiter.
	/// See here for more details: https://www.backblaze.com/b2/docs/b2_list_file_names.html
	/// </summary>
	public async Task<B2FileList> GetListWithPrefixOrDelimiter(string? startFileName = null, string? prefix = null, string? delimiter = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.GetList(_options, operationalBucketId, startFileName, maxFileCount, prefix, delimiter);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2FileList>(response, API);
	}

	/// <summary>
	/// Lists all of the versions of all of the files contained in one bucket,
	/// in alphabetical order by file name, and by reverse of date/time uploaded
	/// for versions of files with the same name.
	/// </summary>
	public async Task<B2FileList> GetVersions(string? startFileName = null, string? startFileId = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default) {
		return await GetVersionsWithPrefixOrDelimiter(startFileName, startFileId, null, null, maxFileCount, bucketId, cancelToken);
	}

	/// <summary>
	/// BETA: Lists all of the versions of all of the files contained in one bucket,
	/// in alphabetical order by file name, and by reverse of date/time uploaded
	/// for versions of files with the same name. With an optional file prefix or delimiter.
	/// See here for more details: https://www.backblaze.com/b2/docs/b2_list_file_versions.html
	/// </summary>
	public async Task<B2FileList> GetVersionsWithPrefixOrDelimiter(string? startFileName = null, string? startFileId = null, string? prefix = null, string? delimiter = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.ListVersions(_options, operationalBucketId, startFileName, startFileId, maxFileCount, prefix, delimiter);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2FileList>(response, API);
	}

	/// <summary>
	/// Gets information about one file stored in B2.
	/// </summary>
	public async Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default) {
		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.GetInfo(_options, fileId);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// get an upload url for use with one Thread.
	/// </summary>
	public async Task<B2UploadUrl> GetUploadUrl(string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		// send the request.
		HttpRequestMessage uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
		HttpResponseMessage uploadUrlResponse = await _client.SendAsync(uploadUrlRequest, cancelToken);

		return await ResponseParser.ParseResponse<B2UploadUrl>(uploadUrlResponse, API);
	}

	/// <summary>
	/// DEPRECATED: This method has been deprecated in favor of the Upload that takes an UploadUrl parameter.
	/// The other Upload method is the preferred, and more efficient way, of uploading to B2.
	/// </summary>
	public async Task<B2File> Upload(byte[] fileData, string fileName, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		// Get the upload url for this file
		B2UploadUrl uploadUrl = await GetUploadUrl(bucketId, cancelToken);

		// Now we can upload the file
		HttpRequestMessage request = FileUploadRequestGenerators.Upload(uploadUrl, fileData, fileName, fileInfo);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded.
	/// </summary>
	public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		return await Upload(fileData, fileName, uploadUrl, null, false, bucketId, fileInfo, cancelToken);
	}

	/// <summary>
	/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
	/// is set true it will retry a failed upload once after 1 second.
	/// </summary>
	public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, bool autoRetry, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		return await Upload(fileData, fileName, uploadUrl, null, autoRetry, bucketId, fileInfo, cancelToken);
	}

	/// <summary>
	/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
	/// is set true it will retry a failed upload once after 1 second.
	/// </summary>
	public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string? contentType, bool autoRetry, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		HttpRequestMessage request = FileUploadRequestGenerators.Upload(uploadUrl, fileData, fileName, fileInfo, contentType);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Auto retry
		if (autoRetry && response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or HttpStatusCode.ServiceUnavailable) {
			await Task.Delay(1000, cancelToken).WaitAsync(cancelToken);
			HttpRequestMessage retryMessage = FileUploadRequestGenerators.Upload(uploadUrl, fileData, fileName, fileInfo, contentType);
			response = await _client.SendAsync(retryMessage, cancelToken);
		}

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Uploads one file to B2 using a stream, returning its unique file ID. Filename will be URL Encoded. If auto retry
	/// is set true it will retry a failed upload once after 1 second. If you don't want to use a SHA1 for the stream set dontSHA.
	/// </summary>
	public async Task<B2File> Upload(Stream fileDataWithSha, string fileName, B2UploadUrl uploadUrl, string? contentType = null, bool autoRetry = false, string? bucketId = null, Dictionary<string, string>? fileInfo = null, bool dontSha = false, CancellationToken cancelToken = default) {
		// Now we can upload the file
		HttpRequestMessage requestMessage = FileUploadRequestGenerators.Upload(uploadUrl, fileDataWithSha, fileName, fileInfo, contentType, dontSha);

		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);
		// Auto retry
		if (autoRetry && response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or HttpStatusCode.ServiceUnavailable) {
			await Task.Delay(1000, cancelToken).WaitAsync(cancelToken);
			HttpRequestMessage retryMessage = FileUploadRequestGenerators.Upload(uploadUrl, fileDataWithSha, fileName, fileInfo, contentType, dontSha);
			response = await _client.SendAsync(retryMessage, cancelToken);
		}

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Downloads a file part by providing the name of the bucket and the name and byte range of the file.
	/// For use with the Large File API.
	/// </summary>
	public async Task<B2File> DownloadByName(string fileName, string bucketName, int startByte, int endByte, CancellationToken cancelToken = default) {
		// Are we searching by name or id?
		HttpRequestMessage request = FileDownloadRequestGenerators.DownloadByName(_options, bucketName, fileName, $"{startByte}-{endByte}");

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ParseDownloadResponse(response);
	}

	/// <summary>
	/// Downloads one file by providing the name of the bucket and the name of the file.
	/// </summary>
	public async Task<B2File> DownloadByName(string fileName, string bucketName, CancellationToken cancelToken = default) {
		// Are we searching by name or id?
		HttpRequestMessage request = FileDownloadRequestGenerators.DownloadByName(_options, bucketName, fileName);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ParseDownloadResponse(response);
	}

	/// <summary>
	/// Downloads a file from B2 using the byte range specified. For use with the Large File API.
	/// </summary>
	public async Task<B2File> DownloadById(string fileId, int startByte, int endByte, CancellationToken cancelToken = default) {
		// Are we searching by name or id?
		HttpRequestMessage request = FileDownloadRequestGenerators.DownloadById(_options, fileId, $"{startByte}-{endByte}");

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ParseDownloadResponse(response);
	}

	/// <summary>
	/// Downloads one file from B2.
	/// </summary>
	public async Task<B2File> DownloadById(string fileId, CancellationToken cancelToken = default) {
		// Are we searching by name or id?
		HttpRequestMessage request = FileDownloadRequestGenerators.DownloadById(_options, fileId);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ParseDownloadResponse(response);
	}

	/// <summary>
	/// Deletes the specified file version
	/// </summary>
	public async Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default) {
		HttpRequestMessage requestMessage = FileDeleteRequestGenerator.Delete(_options, fileId, fileName);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}


	/// <summary>
	/// EXPERIMENTAL: This functionality is not officially part of the Backblaze B2 API and may change or break at any time.
	/// This will return a friendly URL that can be shared to download the file. This depends on the Bucket that the file resides
	/// in to be allPublic.
	/// </summary>
	public string GetFriendlyDownloadUrl(string fileName, string bucketName) {
		return !string.IsNullOrEmpty(_options.DownloadUrl) ? $"{_options.DownloadUrl}/file/{bucketName}/{fileName}" : "";
	}

	/// <summary>
	/// Hides or Shows a file so that downloading by name will not find the file,
	/// but previous versions of the file are still stored. See File
	/// Versions about what it means to hide a file.
	/// </summary>
	public async Task<B2File> Hide(string fileName, string? bucketId = null, string? fileId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.HideFile(_options, operationalBucketId, fileName, fileId);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Copy or Replace a file stored in B2. This will copy the file on B2's servers, resulting in no download or upload charges.
	/// </summary>
	public async Task<B2File> Copy(
		string sourceFileId,
		string fileName,
		B2MetadataDirective metadataDirective = B2MetadataDirective.COPY,
		string? contentType = null,
		Dictionary<string, string>? fileInfo = null,
		string? range = null,
		string? destinationBucketId = null,
		CancellationToken cancelToken = default
	) {
		if (metadataDirective == B2MetadataDirective.COPY && (!string.IsNullOrWhiteSpace(contentType) || fileInfo != null)) {
			throw new CopyReplaceSetupException("Copy operations cannot specify fileInfo or contentType.");
		}

		if (metadataDirective == B2MetadataDirective.REPLACE && (string.IsNullOrWhiteSpace(contentType) || fileInfo == null)) {
			throw new CopyReplaceSetupException("Replace operations must specify fileInfo and contentType.");
		}

		HttpRequestMessage request = FileCopyRequestGenerators.Copy(_options, sourceFileId, fileName, metadataDirective, contentType, fileInfo, range, destinationBucketId);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Downloads one file from B2.
	/// </summary>
	public async Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string? bucketId = null, string? b2ContentDisposition = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = FileDownloadRequestGenerators.GetDownloadAuthorization(_options, fileNamePrefix, validDurationInSeconds, operationalBucketId, b2ContentDisposition);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2DownloadAuthorization>(response, API);
	}

	static async Task<B2File> ParseDownloadResponse(HttpResponseMessage response) {
		await Utils.CheckForErrors(response, API);

		B2File file = new();
		if (response.Headers.TryGetValues("X-Bz-Content-Sha1", out IEnumerable<string>? values)) {
			file.ContentSha1 = values.First();
		}

		if (response.Headers.TryGetValues("X-Bz-File-Name", out values)) {
			file.FileName = values.First();
			// Decode file name
			file.FileName = file.FileName.B2UrlDecode();
		}

		if (response.Headers.TryGetValues("X-Bz-File-Id", out values)) {
			file.FileId = values.First();
		}

		// File Info Headers
		List<KeyValuePair<string, IEnumerable<string>>> fileInfoHeaders = response.Headers.Where(h => h.Key.ToLower().Contains("x-bz-info")).ToList();
		Dictionary<string, string> infoData = new();
		if (fileInfoHeaders.Any()) {
			foreach (KeyValuePair<string, IEnumerable<string>> fileInfo in fileInfoHeaders) {
				// Substring to parse out the file info prefix.
				infoData.Add(fileInfo.Key[10..], fileInfo.Value.First());
			}
		}

		file.FileInfo = infoData;
		if (response.Content.Headers.ContentLength.HasValue) {
			file.Size = response.Content.Headers.ContentLength.Value;
		}

		file.FileData = await response.Content.ReadAsByteArrayAsync();

		return await Task.FromResult(file);
	}
}
