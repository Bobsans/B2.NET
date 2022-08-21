﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using B2.Models;

namespace B2;

public interface IFiles {
	Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default);
	Task<B2File> DownloadById(string fileId, CancellationToken cancelToken = default);
	Task<B2File> DownloadById(string fileId, int startByte, int endByte, CancellationToken cancelToken = default);
	Task<B2File> DownloadByName(string fileName, string bucketName, CancellationToken cancelToken = default);
	Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string bucketId = "", string b2ContentDisposition = "", CancellationToken cancelToken = default);
	Task<B2File> DownloadByName(string fileName, string bucketName, int startByte, int endByte, CancellationToken cancelToken = default);
	Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default);
	Task<B2FileList> GetList(string startFileName = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default);
	Task<B2FileList> GetListWithPrefixOrDelimiter(string startFileName = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default);
	Task<B2UploadUrl> GetUploadUrl(string bucketId = "", CancellationToken cancelToken = default);
	Task<B2FileList> GetVersions(string startFileName = "", string startFileId = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default);
	Task<B2FileList> GetVersionsWithPrefixOrDelimiter(string startFileName = "", string startFileId = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default);
	Task<B2File> Hide(string fileName, string bucketId = "", string fileId = "", CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, bool autoRetry, string bucketId = "", Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(Stream fileDataWithSha, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string bucketId = "", Dictionary<string, string>? fileInfo = null, bool dontSha = false, CancellationToken cancelToken = default);
	Task<B2File> Copy(string sourceFileId, string newFileName, B2MetadataDirective metadataDirective = B2MetadataDirective.COPY, string contentType = "", Dictionary<string, string>? fileInfo = null, string range = "", string destinationBucketId = "", CancellationToken cancelToken = default);
	string GetFriendlyDownloadUrl(string fileName, string bucketName);
}
