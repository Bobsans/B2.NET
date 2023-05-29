using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

[NonParallelizable]
public class BaseTest {
	// TODO: Change these to valid keys to run tests
	protected const string APPLICATION_KEY = "K004dDrvsYE1ofxuPG1JTBTTT71lTWw";
	protected const string APPLICATION_KEY_ID = "00475377ae840cb0000000004";

	protected const string RESTRICTED_APPLICATION_KEY = "K0046+wCsbYFFgmJWZ1VGTC2YUFnYYA";
	protected const string RESTRICTED_APPLICATION_KEY_ID = "00475377ae840cb0000000002";

	public static B2Options DefaultOptions => new() { KeyId = APPLICATION_KEY_ID, ApplicationKey = APPLICATION_KEY };
	public static B2Options DefaultNoAutoAuthOptions => new() { KeyId = APPLICATION_KEY_ID, ApplicationKey = APPLICATION_KEY, AutomaticAuth = false };
	public static B2Options DefaultWithEmptyApplicationIdOptions => new() { KeyId = APPLICATION_KEY_ID, ApplicationKey = "" };
	public static B2Options RestrictedOptions => new() { KeyId = APPLICATION_KEY_ID, ApplicationKey = APPLICATION_KEY };

	protected static B2Options BuildRestrictedOptions(bool autoAuth = true) => new() {
		KeyId = RESTRICTED_APPLICATION_KEY_ID,
		ApplicationKey = RESTRICTED_APPLICATION_KEY,
		AutomaticAuth = autoAuth
	};

	public const string TEST_BUCKET_PREFIX = "B2NETTestingBucket";
	public static string GetNewBucketName() => $"{TEST_BUCKET_PREFIX}-{Path.GetRandomFileName().Replace(".", "")}";

	public static async Task<B2Bucket> CreateBucket(string? name = null, bool isPublic = false) {
		return await new B2Client(DefaultOptions).Buckets.Create(name ?? GetNewBucketName(), isPublic ? BucketType.AllPublic : BucketType.AllPrivate);
	}

	public static async void CleanupBucket(B2Bucket bucket) {
		B2Client client = new(DefaultOptions);

		B2FileList files = await client.Files.GetList(bucketId: bucket.BucketId);

		foreach (B2File file in files.Files) {
			B2FileList versions = await client.Files.GetVersionsWithPrefixOrDelimiter(prefix: file.FileName, bucketId: bucket.BucketId);

			foreach (B2File version in versions.Files) {
				await client.Files.Delete(version.FileId, version.FileName);
			}
		}

		await client.Buckets.Delete(bucket.BucketId);
	}
}
