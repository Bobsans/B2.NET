using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;
using Newtonsoft.Json;

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
	/// <param name="bucketId"></param>
	/// <param name="startFileName"></param>
	/// <param name="maxFileCount"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2FileList> GetList(string startFileName = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default) {
		return await GetListWithPrefixOrDelimiter(startFileName, "", "", maxFileCount, bucketId, cancelToken);
	}

	/// <summary>
	/// BETA: Lists the names of all non-hidden files in a bucket, starting at a given name. With an optional file prefix or delimiter.
	/// See here for more details: https://www.backblaze.com/b2/docs/b2_list_file_names.html
	/// </summary>
	/// <param name="startFileName"></param>
	/// <param name="prefix"></param>
	/// <param name="delimiter"></param>
	/// <param name="maxFileCount"></param>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2FileList> GetListWithPrefixOrDelimiter(string startFileName = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.GetList(_options, operationalBucketId, startFileName, maxFileCount, prefix, delimiter);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2FileList>(response, API);
	}

	/// <summary>
	/// Lists all of the versions of all of the files contained in one bucket,
	/// in alphabetical order by file name, and by reverse of date/time uploaded
	/// for versions of files with the same name.
	/// </summary>
	/// <param name="startFileName"></param>
	/// <param name="startFileId"></param>
	/// <param name="maxFileCount"></param>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2FileList> GetVersions(string startFileName = "", string startFileId = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default) {
		return await GetVersionsWithPrefixOrDelimiter(startFileName, startFileId, "", "", maxFileCount, bucketId, cancelToken);
	}

	/// <summary>
	/// BETA: Lists all of the versions of all of the files contained in one bucket,
	/// in alphabetical order by file name, and by reverse of date/time uploaded
	/// for versions of files with the same name. With an optional file prefix or delimiter.
	/// See here for more details: https://www.backblaze.com/b2/docs/b2_list_file_versions.html
	/// </summary>
	/// <param name="startFileName"></param>
	/// <param name="startFileId"></param>
	/// <param name="prefix"></param>
	/// <param name="delimiter"></param>
	/// <param name="maxFileCount"></param>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2FileList> GetVersionsWithPrefixOrDelimiter(string startFileName = "", string startFileId = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.ListVersions(_options, operationalBucketId, startFileName, startFileId, maxFileCount, prefix, delimiter);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2FileList>(response, API);
	}

	/// <summary>
	/// Gets information about one file stored in B2.
	/// </summary>
	/// <param name="fileId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default) {
		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.GetInfo(_options, fileId);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// get an upload url for use with one Thread.
	/// </summary>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2UploadUrl> GetUploadUrl(string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		// send the request.
		HttpRequestMessage uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
		HttpResponseMessage uploadUrlResponse = await _client.SendAsync(uploadUrlRequest, cancelToken);

		return await ResponseParser.ParseResponse<B2UploadUrl>(uploadUrlResponse);
	}

	/// <summary>
	/// DEPRECATED: This method has been deprecated in favor of the Upload that takes an UploadUrl parameter.
	/// The other Upload method is the preferred, and more efficient way, of uploading to B2.
	/// </summary>
	/// <param name="fileData"></param>
	/// <param name="fileName"></param>
	/// <param name="bucketId"></param>
	/// <param name="fileInfo"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Upload(byte[] fileData, string fileName, string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		// Get the upload url for this file
		HttpRequestMessage uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
		HttpResponseMessage uploadUrlResponse = await _client.SendAsync(uploadUrlRequest, cancelToken);
		string uploadUrlData = await uploadUrlResponse.Content.ReadAsStringAsync(cancelToken);
		B2UploadUrl uploadUrlObject = JsonConvert.DeserializeObject<B2UploadUrl>(uploadUrlData)!;

		// Now we can upload the file
		HttpRequestMessage requestMessage = FileUploadRequestGenerators.Upload(uploadUrlObject, fileData, fileName, fileInfo);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded.
	/// </summary>
	/// <param name="fileData"></param>
	/// <param name="fileName"></param>
	/// <param name="uploadUrl"></param>
	/// <param name="bucketId"></param>
	/// <param name="fileInfo"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		return await Upload(fileData, fileName, uploadUrl, "", false, bucketId, fileInfo, cancelToken);
	}

	/// <summary>
	/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
	/// is set true it will retry a failed upload once after 1 second.
	/// </summary>
	/// <param name="fileData"></param>
	/// <param name="fileName"></param>
	/// <param name="uploadUrl"></param>
	/// <param name="bucketId"></param>
	/// <param name="autoRetry">Retry a failed upload one time after waiting for 1 second.</param>
	/// <param name="fileInfo"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, bool autoRetry, string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		return await Upload(fileData, fileName, uploadUrl, "", autoRetry, bucketId, fileInfo, cancelToken);
	}

	/// <summary>
	/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
	/// is set true it will retry a failed upload once after 1 second.
	/// </summary>
	/// <param name="fileData"></param>
	/// <param name="fileName"></param>
	/// <param name="uploadUrl"></param>
	/// <param name="bucketId"></param>
	/// <param name="contentType"></param>
	/// <param name="autoRetry">Retry a failed upload one time after waiting for 1 second.</param>
	/// <param name="fileInfo"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		// Now we can upload the file
		HttpRequestMessage requestMessage = FileUploadRequestGenerators.Upload(uploadUrl, fileData, fileName, fileInfo, contentType);

		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);
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
	/// <param name="fileDataWithSha"></param>
	/// <param name="fileName"></param>
	/// <param name="uploadUrl"></param>
	/// <param name="contentType"></param>
	/// <param name="autoRetry"></param>
	/// <param name="bucketId"></param>
	/// <param name="fileInfo"></param>
	/// <param name="dontSha"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Upload(Stream fileDataWithSha, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string bucketId = "", Dictionary<string, string>? fileInfo = null, bool dontSha = false, CancellationToken cancelToken = default) {
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
	/// <param name="fileName"></param>
	/// <param name="bucketName"></param>
	/// <param name="startByte"></param>
	/// <param name="endByte"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> DownloadByName(
		string fileName, string bucketName, int startByte, int endByte,
		CancellationToken cancelToken = default
	) {
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
	/// <param name="fileName"></param>
	/// <param name="bucketName"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
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
	/// <param name="fileId"></param>
	/// <param name="startByte"></param>
	/// <param name="endByte"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
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
	/// <param name="fileId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
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
	/// <param name="fileId"></param>
	/// <param name="fileName"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
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
	/// <param name="fileName"></param>
	/// <param name="bucketName"></param>
	/// <returns></returns>
	public string GetFriendlyDownloadUrl(string fileName, string bucketName) {
		return !string.IsNullOrEmpty(_options.DownloadUrl) ? $"{_options.DownloadUrl}/file/{bucketName}/{fileName}" : "";
	}

	/// <summary>
	/// Hides or Unhides a file so that downloading by name will not find the file,
	/// but previous versions of the file are still stored. See File
	/// Versions about what it means to hide a file.
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="bucketId"></param>
	/// <param name="fileId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Hide(string fileName, string bucketId = "", string fileId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = FileMetaDataRequestGenerators.HideFile(_options, operationalBucketId, fileName, fileId);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Copy or Replace a file stored in B2. This will copy the file on B2's servers, resulting in no download or upload charges.
	/// </summary>
	/// <param name="sourceFileId"></param>
	/// <param name="newFileName"></param>
	/// <param name="metadataDirective">COPY or REPLACE. COPY will not allow any changes to File Info or Content Type. REPLACE will.</param>
	/// <param name="contentType"></param>
	/// <param name="fileInfo"></param>
	/// <param name="range">byte range to copy.</param>
	/// <param name="destinationBucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> Copy(
		string sourceFileId,
		string newFileName,
		B2MetadataDirective metadataDirective = B2MetadataDirective.COPY,
		string contentType = "",
		Dictionary<string, string>? fileInfo = null,
		string range = "",
		string destinationBucketId = "",
		CancellationToken cancelToken = default
	) {
		if (metadataDirective == B2MetadataDirective.COPY && (!string.IsNullOrWhiteSpace(contentType) || fileInfo != null)) {
			throw new CopyReplaceSetupException("Copy operations cannot specify fileInfo or contentType.");
		}

		if (metadataDirective == B2MetadataDirective.REPLACE && (string.IsNullOrWhiteSpace(contentType) || fileInfo == null)) {
			throw new CopyReplaceSetupException("Replace operations must specify fileInfo and contentType.");
		}

		HttpRequestMessage request = FileCopyRequestGenerators.Copy(_options, sourceFileId, newFileName, metadataDirective, contentType, fileInfo, range, destinationBucketId);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Downloads one file from B2.
	/// </summary>
	/// <param name="fileNamePrefix"></param>
	/// <param name="validDurationInSeconds"></param>
	/// <param name="bucketId"></param>
	/// <param name="b2ContentDisposition"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string bucketId = "", string b2ContentDisposition = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = FileDownloadRequestGenerators.GetDownloadAuthorization(_options, fileNamePrefix, validDurationInSeconds, operationalBucketId, b2ContentDisposition);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2DownloadAuthorization>(response, API);
	}

	static async Task<B2File> ParseDownloadResponse(HttpResponseMessage response) {
		await Utilities.CheckForErrors(response, API);

		B2File file = new();
		if (response.Headers.TryGetValues("X-Bz-Content-Sha1", out IEnumerable<string>? values)) {
			file.ContentSHA1 = values.First();
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
				infoData.Add(fileInfo.Key.Substring(10), fileInfo.Value.First());
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
