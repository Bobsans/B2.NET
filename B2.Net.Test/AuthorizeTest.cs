using B2.Models;

namespace B2.Test;

public class AuthorizeTest : BaseTest {
	[Test]
	public void IsAutomaticAuthorize() {
		B2Options options = DefaultOptions;
		B2Client _ = new(options);

		Assert.That(string.IsNullOrEmpty(options.AuthorizationToken), Is.False);
	}

	[Test]
	public void IsManualAuthorize() {
		B2Options options = DefaultNoAutoAuthOptions;
		B2Client _ = new B2Client(options).Authorize();

		Assert.That(string.IsNullOrEmpty(options.AuthorizationToken), Is.False);
	}

	[Test]
	public void GetCapabilitiesOnApplicationKey() {
		B2Options options = DefaultOptions;
		B2Client _ = new(options);

		Assert.Multiple(() => {
			Assert.That(string.IsNullOrEmpty(options.AuthorizationToken), Is.False);
			Assert.That(options.Capabilities, Is.Not.Null);
			Assert.That(options.Capabilities.Capabilities, Is.Not.Null);
		});
	}

	[Test]
	public void ErrorAuthorizeNonMasterKeyWithMissingKeyId() {
		Assert.ThrowsAsync<AuthorizationException>(async () => {
			await new B2Client(new B2Options {
				ApplicationKey = "K001LarMmmWDIveFaZz3yvB4uattO+Q",
				KeyId = "",
				AutomaticAuth = false
			}).AuthorizeAsync();
		});
	}

	[Test]
	public void IsOptionsFillFromResponse() {
		B2Options options = DefaultOptions;
		B2Client _ = new(options);

		Assert.Multiple(() => {
			Assert.That(options.AbsoluteMinimumPartSize, Is.Not.EqualTo(0));
			Assert.That(options.MinimumPartSize, Is.Not.EqualTo(0));
			Assert.That(options.RecommendedPartSize, Is.Not.EqualTo(0));
			Assert.That(string.IsNullOrEmpty(options.DownloadUrl), Is.False);
			Assert.That(string.IsNullOrEmpty(options.ApiUrl), Is.False);
		});
	}
}
