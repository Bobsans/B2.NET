using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;

namespace B2;

public class LargeFiles : ILargeFiles {
	const string API = "Large Files";

	readonly B2Options _options;
	readonly HttpClient _client;

	public LargeFiles(B2Options options) {
		_options = options;
		_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
	}

	/// <summary>
	/// Starts a large file upload.
	/// </summary>
	public async Task<B2File> StartLargeFile(string fileName, string? contentType = null, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);
		HttpRequestMessage request = LargeFileRequestGenerators.Start(_options, operationalBucketId, fileName, contentType, fileInfo);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Get an upload url for use with one Thread.
	/// </summary>
	public async Task<B2UploadPartUrl> GetUploadPartUrl(string fileId, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.GetUploadPartUrl(_options, fileId);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2UploadPartUrl>(response, API);
	}

	/// <summary>
	/// Upload one part of an already started large file upload.
	/// </summary>
	public async Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.Upload(_options, fileData, partNumber, uploadPartUrl);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2UploadPart>(response, API);
	}

	/// <summary>
	/// Downloads one file by providing the name of the bucket and the name of the file.
	/// </summary>
	public async Task<B2File> FinishLargeFile(string fileId, string[] partSha1Array, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.Finish(_options, fileId, partSha1Array);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// List the parts of an incomplete large file upload.
	/// </summary>
	public async Task<B2LargeFileParts> ListPartsForIncompleteFile(string fileId, int startPartNumber, int maxPartCount, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.ListParts(_options, fileId, startPartNumber, maxPartCount);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2LargeFileParts>(response, API);
	}

	/// <summary>
	/// Cancel a large file upload
	/// </summary>
	public async Task<B2CancelledFile> CancelLargeFile(string fileId, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.Cancel(_options, fileId);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2CancelledFile>(response, API);
	}

	/// <summary>
	/// List all the incomplete large file uploads for the supplied bucket
	/// </summary>
	public async Task<B2IncompleteLargeFiles> ListIncompleteFiles(string bucketId, string? startFileId = null, int? maxFileCount = null, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.IncompleteFiles(_options, bucketId, startFileId, maxFileCount);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2IncompleteLargeFiles>(response, API);
	}

	/// <summary>
	/// Copy a source file into part of a large file
	/// </summary>
	public async Task<B2LargeFilePart> CopyPart(string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string? range = null, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.CopyPart(_options, sourceFileId, destinationLargeFileId, destinationPartNumber, range);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);
		return await ResponseParser.ParseResponse<B2LargeFilePart>(response, API);
	}
}
