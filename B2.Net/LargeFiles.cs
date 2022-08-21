using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
	/// <param name="fileName"></param>
	/// <param name="contentType"></param>
	/// <param name="bucketId"></param>
	/// <param name="fileInfo"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> StartLargeFile(string fileName, string contentType = "", string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = LargeFileRequestGenerators.Start(_options, operationalBucketId, fileName, contentType, fileInfo);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// Get an upload url for use with one Thread.
	/// </summary>
	/// <param name="fileId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2UploadPartUrl> GetUploadPartUrl(string fileId, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.GetUploadPartUrl(_options, fileId);

		HttpResponseMessage uploadUrlResponse = await _client.SendAsync(request, cancelToken);

		B2UploadPartUrl uploadUrl = await ResponseParser.ParseResponse<B2UploadPartUrl>(uploadUrlResponse, API);

		return uploadUrl;
	}

	/// <summary>
	/// Upload one part of an already started large file upload.
	/// </summary>
	/// <param name="fileData"></param>
	/// <param name="partNumber"></param>
	/// <param name="uploadPartUrl"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.Upload(_options, fileData, partNumber, uploadPartUrl);

		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2UploadPart>(response, API);
	}

	/// <summary>
	/// Downloads one file by providing the name of the bucket and the name of the file.
	/// </summary>
	/// <param name="fileId"></param>
	/// <param name="partSha1Array"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2File> FinishLargeFile(string fileId, string[] partSha1Array, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.Finish(_options, fileId, partSha1Array);

		// Send the request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2File>(response, API);
	}

	/// <summary>
	/// List the parts of an incomplete large file upload.
	/// </summary>
	/// <param name="fileId"></param>
	/// <param name="startPartNumber"></param>
	/// <param name="maxPartCount"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2LargeFileParts> ListPartsForIncompleteFile(string fileId, int startPartNumber, int maxPartCount, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.ListParts(_options, fileId, startPartNumber, maxPartCount);

		// Send the request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2LargeFileParts>(response, API);
	}

	/// <summary>
	/// Cancel a large file upload
	/// </summary>
	/// <param name="fileId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2CancelledFile> CancelLargeFile(string fileId, CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.Cancel(_options, fileId);

		// Send the request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2CancelledFile>(response, API);
	}

	/// <summary>
	/// List all the incomplete large file uploads for the supplied bucket
	/// </summary>
	/// <param name="bucketId"></param>
	/// <param name="startFileId"></param>
	/// <param name="maxFileCount"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2IncompleteLargeFiles> ListIncompleteFiles(string bucketId, string startFileId = "", string maxFileCount = "", CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.IncompleteFiles(_options, bucketId, startFileId, maxFileCount);

		// Send the request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2IncompleteLargeFiles>(response, API);
	}

	/// <summary>
	/// Copy a source file into part of a large file
	/// </summary>
	/// <param name="sourceFileId"></param>
	/// <param name="destinationLargeFileId"></param>
	/// <param name="destinationPartNumber"></param>
	/// <param name="range"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2LargeFilePart> CopyPart(string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string range = "", CancellationToken cancelToken = default) {
		HttpRequestMessage request = LargeFileRequestGenerators.CopyPart(_options, sourceFileId, destinationLargeFileId, destinationPartNumber, range);

		// Send the download request
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		// Create B2File from response
		return await ResponseParser.ParseResponse<B2LargeFilePart>(response, API);
	}
}
