using B2.Models;

namespace B2;

public interface IBuckets {
	Task<B2Bucket> Create(string bucketName, B2BucketOptions options, CancellationToken cancelToken = default);
	Task<B2Bucket> Create(string bucketName, BucketType bucketType, CancellationToken cancelToken = default);
	Task<B2Bucket> Delete(string? bucketId = null, CancellationToken cancelToken = default);
	Task<List<B2Bucket>> GetList(CancellationToken cancelToken = default);
	Task<B2Bucket> Update(BucketType bucketType, string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2Bucket> Update(B2BucketOptions options, string? bucketId = null, CancellationToken cancelToken = default);
	Task<B2Bucket> Update(B2BucketOptions options, int revisionNumber, string? bucketId = null, CancellationToken cancelToken = default);
}
