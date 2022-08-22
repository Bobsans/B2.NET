using B2.Models;

namespace B2;

public interface IFiles {
	Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default);
	Task<B2File> DownloadById(string fileId, CancellationToken cancelToken = default);
	Task<B2File> DownloadById(string fileId, int startByte, int endByte, CancellationToken cancelToken = default);
	Task<B2File> DownloadByName(string fileName, string bucketName, CancellationToken cancelToken = default);
	Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string? bucketId = null, string? b2ContentDisposition = null, CancellationToken cancelToken = default);
	Task<B2File> DownloadByName(string fileName, string bucketName, int startByte, int endByte, CancellationToken cancelToken = default);
	Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default);
	Task<B2FileList> GetList(string? startFileName = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2FileList> GetListWithPrefixOrDelimiter(string? startFileName = null, string? prefix = null, string? delimiter = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2UploadUrl> GetUploadUrl(string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2FileList> GetVersions(string? startFileName = null, string? startFileId = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2FileList> GetVersionsWithPrefixOrDelimiter(string? startFileName = null, string? startFileId = null, string? prefix = null, string? delimiter = null, int? maxFileCount = null, string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2File> Hide(string fileName, string? bucketId = null, string? fileId = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, bool autoRetry, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2File> Upload(Stream fileDataWithSha, string fileName, B2UploadUrl uploadUrl, string? contentType = null, bool autoRetry = false, string? bucketId = null, Dictionary<string, string>? fileInfo = null, bool dontSha = false, CancellationToken cancelToken = default);
	Task<B2File> Copy(string sourceFileId, string fileName, B2MetadataDirective metadataDirective = B2MetadataDirective.COPY, string? contentType = null, Dictionary<string, string>? fileInfo = null, string? range = null, string? destinationBucketId = null, CancellationToken cancelToken = default);

	// Experimental
	string GetFriendlyDownloadUrl(string fileName, string bucketName);
}
