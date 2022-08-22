using B2.Models;

namespace B2;

public interface ILargeFiles {
	Task<B2CancelledFile> CancelLargeFile(string fileId, CancellationToken cancelToken = default);
	Task<B2File> FinishLargeFile(string fileId, string[] partSha1Array, CancellationToken cancelToken = default);
	Task<B2UploadPartUrl> GetUploadPartUrl(string fileId, CancellationToken cancelToken = default);
	Task<B2IncompleteLargeFiles> ListIncompleteFiles(string bucketId, string? startFileId = null, int? maxFileCount = null, CancellationToken cancelToken = default);
	Task<B2LargeFileParts> ListPartsForIncompleteFile(string fileId, int startPartNumber, int maxPartCount, CancellationToken cancelToken = default);
	Task<B2File> StartLargeFile(string fileName, string? contentType = null, string? bucketId = null, Dictionary<string, string>? fileInfo = null, CancellationToken cancelToken = default);
	Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl, CancellationToken cancelToken = default);
	Task<B2LargeFilePart> CopyPart(string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string? range = null, CancellationToken cancelToken = default);
}
