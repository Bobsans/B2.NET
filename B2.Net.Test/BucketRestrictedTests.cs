using System.IO;
using B2.Models;

namespace B2.Test;

public class BucketRestrictedTests : BaseTest {
	string bucketName = "";

	[Test]
	public void GetBucketListTest() {
		// Key that is restricted to a specific bucket name above.
		B2Client client = new(B2Client.Authorize(new B2Options {
			KeyId = RESTRICTED_APPLICATION_KEY_ID,
			ApplicationKey = RESTRICTED_APPLICATION_KEY
		}));
		bucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "")[..6]}";

		Assert.ThrowsAsync<B2Exception>(async () => {
			await client.Buckets.Create(bucketName, BucketType.allPrivate);
		}, "Unauthorized error when operating on Buckets. Are you sure the key you are using has access?");
	}

	[Test]
	public void BadInitialization() {
		Assert.ThrowsAsync<AuthorizationException>(async () => {
			// Missing AccountId
			await B2Client.AuthorizeAsync(new B2Options {
				KeyId = APPLICATION_KEY_ID,
				ApplicationKey = ""
			});
		}, "Either KeyId or ApplicationKey were not specified.");
	}
}
