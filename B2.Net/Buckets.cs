using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;

namespace B2;

public class Buckets : IBuckets {
	const string API = "Buckets";

	readonly B2Options _options;
	readonly HttpClient _client;

	public Buckets(B2Options options) {
		_options = options;
		_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
	}

	public async Task<List<B2Bucket>> GetList(CancellationToken cancelToken = default) {
		HttpRequestMessage requestMessage = BucketRequestGenerators.GetBucketList(_options);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		B2BucketListDeserializeModel bucketList = await ResponseParser.ParseResponse<B2BucketListDeserializeModel>(response);
		return bucketList.Buckets;
	}

	/// <summary>
	/// Creates a new bucket. A bucket belongs to the account used to create it. If BucketType is not set allPrivate will be used by default.
	/// </summary>
	/// <param name="bucketName"></param>
	/// <param name="bucketType"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2Bucket> Create(
		string bucketName, BucketType bucketType,
		CancellationToken cancelToken = default
	) {
		HttpRequestMessage requestMessage = BucketRequestGenerators.CreateBucket(_options, bucketName, bucketType.ToString());
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Creates a new bucket. A bucket belongs to the account used to create it. If BucketType is not set allPrivate will be used by default.
	/// Use this method to set Cache-Control.
	/// </summary>
	/// <param name="bucketName"></param>
	/// <param name="options"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2Bucket> Create(string bucketName, B2BucketOptions options, CancellationToken cancelToken = default) {
		HttpRequestMessage requestMessage = BucketRequestGenerators.CreateBucket(_options, bucketName, options);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Deletes the bucket specified. Only buckets that contain no version of any files can be deleted.
	/// </summary>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2Bucket> Delete(string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = BucketRequestGenerators.DeleteBucket(_options, operationalBucketId);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
	/// </summary>
	/// <param name="bucketType"></param>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2Bucket> Update(BucketType bucketType, string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);
		HttpRequestMessage requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, bucketType.ToString());
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
	/// Use this method to set Cache-Control, Lifecycle Rules, or CORS rules.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2Bucket> Update(B2BucketOptions options, string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, options);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}

	/// <summary>
	/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
	/// Use this method to set Cache-Control, Lifecycle Rules, or CORS rules.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="revisionNumber"></param>
	/// <param name="bucketId"></param>
	/// <param name="cancelToken"></param>
	/// <returns></returns>
	public async Task<B2Bucket> Update(B2BucketOptions options, int revisionNumber, string bucketId = "", CancellationToken cancelToken = default) {
		string operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

		HttpRequestMessage requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, options, revisionNumber);
		HttpResponseMessage response = await _client.SendAsync(requestMessage, cancelToken);

		return await ResponseParser.ParseResponse<B2Bucket>(response, API);
	}
}
