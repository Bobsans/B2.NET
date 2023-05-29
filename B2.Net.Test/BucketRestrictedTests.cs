using B2.Models;

namespace B2.Test;

public class BucketRestrictedTests : BaseTest {
	[Test]
	public void GetBucketList() {
		// Key that is restricted to a specific bucket name above.
		B2Client client = new(BuildRestrictedOptions());

		Assert.ThrowsAsync<B2Exception>(async () => {
			await client.Buckets.Create(GetNewBucketName(), BucketType.AllPrivate);
		}, "Unauthorized error when operating on Buckets. Are you sure the key you are using has access?");
	}
}
