using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using B2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace B2.Http.RequestGenerators; 

public static class BucketRequestGenerators {
	static class Endpoints {
		public const string LIST = "b2_list_buckets";
		public const string CREATE = "b2_create_bucket";
		public const string DELETE = "b2_delete_bucket";
		public const string UPDATE = "b2_update_bucket";
	}

	public static HttpRequestMessage GetBucketList(B2Options options) {
		string json = JsonConvert.SerializeObject(new { accountId = options.AccountId });
		return BaseRequestGenerator.PostRequest(Endpoints.LIST, json, options);
	}

	public static HttpRequestMessage DeleteBucket(B2Options options, string bucketId) {
		string json = JsonConvert.SerializeObject(new { accountId = options.AccountId, bucketId });
		return BaseRequestGenerator.PostRequest(Endpoints.DELETE, json, options);
	}

	/// <summary>
	/// Create a bucket. Defaults to allPrivate.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="bucketName"></param>
	/// <param name="bucketType"></param>
	/// <returns></returns>
	public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, string bucketType = "allPrivate") {
		Regex allowed = new("^[a-zA-Z0-9-]+$");
		if (bucketName.Length is < 6 or > 50 || !allowed.IsMatch(bucketName) || bucketName.StartsWith("b2-")) {
			throw new Exception(@"The bucket name specified does not match the requirements. 
                            Bucket Name can consist of upper-case letters, lower-case letters, numbers, and "" - "", 
                            must be at least 6 characters long, and can be at most 50 characters long");
		}

		string json = JsonConvert.SerializeObject(new { accountId = options.AccountId, bucketName, bucketType });
		return BaseRequestGenerator.PostRequest(Endpoints.CREATE, json, options);
	}

	/// <summary>
	/// Create a bucket. Defaults to allPrivate.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="bucketName"></param>
	/// <param name="bucketOptions"></param>
	/// <returns></returns>
	public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, B2BucketOptions bucketOptions) {
		// Check lifecycle rules
		bool hasLifecycleRules = bucketOptions.LifecycleRules is { Count: > 0 };
		if (hasLifecycleRules) {
			foreach (B2BucketLifecycleRule rule in bucketOptions.LifecycleRules) {
				if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
					throw new Exception("The smallest number of days you can set in a lifecycle rule is 1.");
				}
				if (rule.DaysFromHidingToDeleting == null && rule.DaysFromUploadingToHiding == null) {
					throw new Exception("You must set either DaysFromHidingToDeleting or DaysFromUploadingToHiding. Both cannot be null.");
				}
			}
		}

		Regex allowed = new("^[a-zA-Z0-9-]+$");
		if (bucketName.Length < 6 || bucketName.Length > 50 || !allowed.IsMatch(bucketName) || bucketName.StartsWith("b2-")) {
			throw new Exception(@"The bucket name specified does not match the requirements. 
                            Bucket Name can consist of upper-case letters, lower-case letters, numbers, and "" - "", 
                            must be at least 6 characters long, and can be at most 50 characters long");
		}

		B2BucketCreateModel body = new() {
			accountId = options.AccountId,
			bucketName = bucketName,
			bucketType = bucketOptions.BucketType.ToString()
		};

		// Add optional options
		if (bucketOptions.CacheControl != 0) {
			body.bucketInfo = new Dictionary<string, string>() {
				{ "Cache-Control", "max-age=" + bucketOptions.CacheControl }
			};
		}
		if (hasLifecycleRules) {
			body.lifecycleRules = bucketOptions.LifecycleRules;
		}

		// Has cors rules
		if (bucketOptions.CORSRules != null && bucketOptions.CORSRules.Count > 0) {
			body.corsRules = bucketOptions.CORSRules;
		}

		string json = JsonSerialize(body);
		return BaseRequestGenerator.PostRequest(Endpoints.CREATE, json, options);
	}

	/// <summary>
	/// Used to modify the bucket type of the provided bucket.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="bucketId"></param>
	/// <param name="bucketType"></param>
	/// <returns></returns>
	public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, string bucketType) {
		string json = JsonConvert.SerializeObject(new { accountId = options.AccountId, bucketId, bucketType });
		return BaseRequestGenerator.PostRequest(Endpoints.UPDATE, json, options);
	}

	/// <summary>
	/// Used to modify the bucket type of the provided bucket.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="bucketId"></param>
	/// <param name="bucketOptions"></param>
	/// <param name="revisionNumber">(optional) When set, the update will only happen if the revision number stored in the B2 service matches the one passed in. This can be used to avoid having simultaneous updates make conflicting changes. </param>
	/// <returns></returns>
	public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, B2BucketOptions bucketOptions, int? revisionNumber = null) {
		// Check lifecycle rules
		bool hasLifecycleRules = bucketOptions.LifecycleRules is { Count: > 0 };
		if (hasLifecycleRules) {
			foreach (B2BucketLifecycleRule rule in bucketOptions.LifecycleRules) {
				if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
					throw new Exception("The smallest number of days you can set in a lifecycle rule is 1.");
				}
				if (rule.DaysFromHidingToDeleting == null && rule.DaysFromUploadingToHiding == null) {
					throw new Exception("You must set either DaysFromHidingToDeleting or DaysFromUploadingToHiding. Both cannot be null.");
				}
			}
		}

		B2BucketUpdateModel body = new() {
			accountId = options.AccountId,
			bucketId = bucketId,
			bucketType = bucketOptions.BucketType.ToString()
		};

		// Add optional options
		if (bucketOptions.CacheControl != 0) {
			body.bucketInfo = new Dictionary<string, string> {
				{ "Cache-Control", "max-age=" + bucketOptions.CacheControl }
			};
		}
		if (hasLifecycleRules) {
			body.lifecycleRules = bucketOptions.LifecycleRules;
		}

		// Has cors rules
		if (bucketOptions.CORSRules is { Count: > 0 }) {
			if (bucketOptions.CORSRules.Any(x => x.AllowedOperations == null || x.AllowedOperations.Length == 0)) {
				throw new Exception("You must set allowedOperations on the bucket CORS rules.");
			}
			if (bucketOptions.CORSRules.Any(x => x.AllowedOrigins == null || x.AllowedOrigins.Length == 0)) {
				throw new Exception("You must set allowedOrigins on the bucket CORS rules.");
			}
			if (bucketOptions.CORSRules.Any(x => string.IsNullOrEmpty(x.CorsRuleName))) {
				throw new Exception("You must set corsRuleName on the bucket CORS rules.");
			}
			body.corsRules = bucketOptions.CORSRules;
		}

		if (revisionNumber.HasValue) {
			body.ifRevisionIs = revisionNumber.Value;
		}

		string json = JsonSerialize(body);
		return BaseRequestGenerator.PostRequest(Endpoints.UPDATE, json, options);
	}

	static string JsonSerialize(object data) {
		return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings() {
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		});
	}
}

class B2BucketCreateModel {
	public string accountId { get; set; }
	public string bucketName { get; set; }
	public string bucketType { get; set; }
	public Dictionary<string, string> bucketInfo { get; set; }
	public List<B2BucketLifecycleRule> lifecycleRules { get; set; }
	public List<B2CorsRule> corsRules { get; set; }
}

class B2BucketUpdateModel {
	public string accountId { get; set; }
	public string bucketId { get; set; }
	public string bucketType { get; set; }
	public Dictionary<string, string> bucketInfo { get; set; }
	public List<B2BucketLifecycleRule> lifecycleRules { get; set; }
	public List<B2CorsRule> corsRules { get; set; }
	public int? ifRevisionIs { get; set; }
}
