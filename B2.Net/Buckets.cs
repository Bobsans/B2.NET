using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;

namespace B2;

public class Buckets {
	const string API = "Buckets";

	readonly B2Options _options;
	readonly HttpClient _client;

	public Buckets(B2Options options) {
		_options = options;
		_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
	}

	public async Task<List<B2Bucket>> GetList(CancellationToken cancelToken = default) {
		HttpRequestMessage request = BucketRequestGenerators.GetBucketList(_options);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		B2BucketListDeserializeModel bucketList = await ResponseParser.ParseResponse<B2BucketListDeserializeModel>(response, API);
		return bucketList.Buckets;
	}

	/// <summary>
	/// Creates a new bucket. A bucket belongs to the account used to create it. If BucketType is not set allPrivate will be used by default.
	/// </summary>
	public async Task<B2Bucket> Create(string bucketName, BucketType bucketType, CancellationToken cancelToken = default) {
		HttpRequestMessage request = BucketRequestGenerators.CreateBucket(_options, bucketName, bucketType);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Creates a new bucket. A bucket belongs to the account used to create it. If BucketType is not set allPrivate will be used by default.
	/// Use this method to set Cache-Control.
	/// </summary>
	public async Task<B2Bucket> Create(string bucketName, B2BucketOptions options, CancellationToken cancelToken = default) {
		HttpRequestMessage request = BucketRequestGenerators.CreateBucket(_options, bucketName, options);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Deletes the bucket specified. Only buckets that contain no version of any files can be deleted.
	/// </summary>
	public async Task<B2Bucket> Delete(string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = BucketRequestGenerators.DeleteBucket(_options, operationalBucketId);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
	/// </summary>
	public async Task<B2Bucket> Update(BucketType bucketType, string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, bucketType);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
	/// Use this method to set Cache-Control, Lifecycle Rules, or CORS rules.
	/// </summary>
	public async Task<B2Bucket> Update(B2BucketOptions options, string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, options);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
	/// Use this method to set Cache-Control, Lifecycle Rules, or CORS rules.
	/// </summary>
	public async Task<B2Bucket> Update(B2BucketOptions options, int revisionNumber, string? bucketId = null, CancellationToken cancelToken = default) {
		string operationalBucketId = Utils.DetermineBucketId(_options, bucketId);

		HttpRequestMessage request = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, options, revisionNumber);
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}
}
