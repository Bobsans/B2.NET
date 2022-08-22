using System.Text.RegularExpressions;
using B2.Models;

namespace B2.Http.RequestGenerators;

public static class BucketRequestGenerators {
	static class Endpoints {
		public const string LIST = "b2_list_buckets";
		public const string CREATE = "b2_create_bucket";
		public const string DELETE = "b2_delete_bucket";
		public const string UPDATE = "b2_update_bucket";
	}

	public static HttpRequestMessage GetBucketList(B2Options options) {
		string json = Utils.Serialize(new { accountId = options.AccountId });
		return BaseRequestGenerator.PostRequest(Endpoints.LIST, json, options);
	}

	public static HttpRequestMessage DeleteBucket(B2Options options, string bucketId) {
		string json = Utils.Serialize(new { accountId = options.AccountId, bucketId });
		return BaseRequestGenerator.PostRequest(Endpoints.DELETE, json, options);
	}

	/// <summary>
	/// Create a bucket. Defaults to allPrivate.
	/// </summary>
	public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, string bucketType = "allPrivate") {
		Regex allowed = new("^[a-zA-Z0-9-]+$");
		if (bucketName.Length is < 6 or > 50 || !allowed.IsMatch(bucketName) || bucketName.StartsWith("b2-")) {
			throw new Exception(
				"The bucket name specified does not match the requirements. Bucket Name can consist of upper-case " +
				"letters, lower-case letters, numbers, and \" - \", must be at least 6 characters long, and can be at most " +
				"50 characters long"
			);
		}

		string json = Utils.Serialize(new { accountId = options.AccountId, bucketName, bucketType });
		return BaseRequestGenerator.PostRequest(Endpoints.CREATE, json, options);
	}

	/// <summary>
	/// Create a bucket. Defaults to allPrivate.
	/// </summary>
	public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, B2BucketOptions bucketOptions) {
		// Check lifecycle rules
		bool hasLifecycleRules = bucketOptions.LifecycleRules is { Count: > 0 };
		if (hasLifecycleRules) {
			foreach (B2BucketLifecycleRule rule in bucketOptions.LifecycleRules) {
				if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
					throw new Exception("The smallest number of days you can set in a lifecycle rule is 1.");
				}

				if (rule.DaysFromHidingToDeleting == null && rule.DaysFromUploadingToHiding == null) {
					throw new Exception(
						"You must set either DaysFromHidingToDeleting or DaysFromUploadingToHiding. " +
						"Both cannot be null."
					);
				}
			}
		}

		Regex allowed = new("^[a-zA-Z0-9-]+$");
		if (bucketName.Length is < 6 or > 50 || !allowed.IsMatch(bucketName) || bucketName.StartsWith("b2-")) {
			throw new Exception(
				"The bucket name specified does not match the requirements. Bucket Name can consist of upper-case " +
				"letters, lower-case letters, numbers, and \" - \", must be at least 6 characters long, and can be at most " +
				"50 characters long"
			);
		}

		B2BucketCreateModel body = new() {
			AccountId = options.AccountId,
			BucketName = bucketName,
			BucketType = bucketOptions.BucketType.ToString()
		};

		// Add optional options
		if (bucketOptions.CacheControl != 0) {
			body.BucketInfo = new Dictionary<string, string> {
				{ "Cache-Control", "max-age=" + bucketOptions.CacheControl }
			};
		}

		if (hasLifecycleRules) {
			body.LifecycleRules = bucketOptions.LifecycleRules;
		}

		// Has cors rules
		if (bucketOptions.CorsRules is { Count: > 0 }) {
			body.CorsRules = bucketOptions.CorsRules;
		}

		return BaseRequestGenerator.PostRequest(Endpoints.CREATE, Utils.Serialize(body), options);
	}

	/// <summary>
	/// Used to modify the bucket type of the provided bucket.
	/// </summary>
	public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, string bucketType) {
		string json = Utils.Serialize(new { accountId = options.AccountId, bucketId, bucketType });
		return BaseRequestGenerator.PostRequest(Endpoints.UPDATE, json, options);
	}

	/// <summary>
	/// Used to modify the bucket type of the provided bucket.
	/// </summary>
	public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, B2BucketOptions bucketOptions, int? revisionNumber = null) {
		bool hasLifecycleRules = bucketOptions.LifecycleRules is { Count: > 0 };

		if (hasLifecycleRules) {
			foreach (B2BucketLifecycleRule rule in bucketOptions.LifecycleRules) {
				if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
					throw new Exception("The smallest number of days you can set in a lifecycle rule is 1.");
				}

				if (rule.DaysFromHidingToDeleting == null && rule.DaysFromUploadingToHiding == null) {
					throw new Exception(
						"You must set either DaysFromHidingToDeleting or DaysFromUploadingToHiding. " +
						"Both cannot be null."
					);
				}
			}
		}

		B2BucketUpdateModel body = new() {
			AccountId = options.AccountId,
			BucketId = bucketId,
			BucketType = bucketOptions.BucketType.ToString()
		};

		// Add optional options
		if (bucketOptions.CacheControl != 0) {
			body.BucketInfo = new Dictionary<string, string> {
				{ "Cache-Control", "max-age=" + bucketOptions.CacheControl }
			};
		}

		if (hasLifecycleRules) {
			body.LifecycleRules = bucketOptions.LifecycleRules;
		}

		// Has cors rules
		if (bucketOptions.CorsRules is { Count: > 0 }) {
			if (bucketOptions.CorsRules.Any(x => x.AllowedOperations.Length == 0)) {
				throw new Exception("You must set allowedOperations on the bucket CORS rules.");
			}

			if (bucketOptions.CorsRules.Any(x => x.AllowedOrigins.Length == 0)) {
				throw new Exception("You must set allowedOrigins on the bucket CORS rules.");
			}

			if (bucketOptions.CorsRules.Any(x => string.IsNullOrEmpty(x.CorsRuleName))) {
				throw new Exception("You must set corsRuleName on the bucket CORS rules.");
			}

			body.CorsRules = bucketOptions.CorsRules;
		}

		if (revisionNumber.HasValue) {
			body.IfRevisionIs = revisionNumber.Value;
		}

		return BaseRequestGenerator.PostRequest(Endpoints.UPDATE, Utils.Serialize(body), options);
	}
}

class B2BucketCreateModel {
	public string AccountId { get; set; } = null!;
	public string BucketName { get; set; } = null!;
	public string BucketType { get; set; } = null!;
	public Dictionary<string, string> BucketInfo { get; set; } = null!;
	public List<B2BucketLifecycleRule> LifecycleRules { get; set; } = null!;
	public List<B2CorsRule> CorsRules { get; set; } = null!;
}

class B2BucketUpdateModel {
	public string AccountId { get; set; } = null!;
	public string BucketId { get; set; } = null!;
	public string BucketType { get; set; } = null!;
	public Dictionary<string, string> BucketInfo { get; set; } = null!;
	public List<B2BucketLifecycleRule> LifecycleRules { get; set; } = null!;
	public List<B2CorsRule> CorsRules { get; set; } = null!;
	public int? IfRevisionIs { get; set; }
}
