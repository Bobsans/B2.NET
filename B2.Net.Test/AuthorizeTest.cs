using System;
using System.Text.Json;
using B2.Models;

namespace B2.Test;

public class AuthorizeTest : BaseTest {
	[Test]
	public void CanWeAuthorize() {
		B2Client client = new(Options);
		B2Options result = client.Authorize().Result;

		Assert.That(string.IsNullOrEmpty(result.AuthorizationToken), Is.False);
	}

	[Test]
	public void CanWeAuthorizeStatic() {
		B2Options result = B2Client.Authorize(Options);
		Console.WriteLine(JsonSerializer.Serialize(result));

		Assert.That(string.IsNullOrEmpty(result.AuthorizationToken), Is.False);
	}

	[Test]
	public void CanWeAuthorizeNonMasterKey() {
		B2Options result = B2Client.Authorize(APPLICATION_KEY_ID, APPLICATION_KEY);
		Console.WriteLine(JsonSerializer.Serialize(result));

		Assert.That(string.IsNullOrEmpty(result.AuthorizationToken), Is.False);
	}

	[Test]
	public void DoWeGetCapabilitiesOnApplicationKey() {
		B2Options result = B2Client.Authorize(APPLICATION_KEY_ID, APPLICATION_KEY);

		Assert.Multiple(() => {
			Assert.That(string.IsNullOrEmpty(result.AuthorizationToken), Is.False);
			Assert.That(result.Capabilities, Is.Not.Null);
			Assert.That(result.Capabilities.Capabilities, Is.Not.Null);
		});
	}

	[Test]
	public void DoWeGetCapabilitiesOnClientWithApplicationKey() {
		B2Client client = new(B2Client.Authorize(APPLICATION_KEY_ID, APPLICATION_KEY));

		Assert.That(client.Capabilities.Capabilities, Is.Not.Null);
	}

	[Test]
	public void ErrorAuthorizeNonMasterKeyWithMissingKeyId() {
		const string key = "K001LarMmmWDIveFaZz3yvB4uattO+Q";

		Assert.ThrowsAsync<AuthorizationException>(async () => {
			await B2Client.AuthorizeAsync("", key);
		});
	}

	[Test]
	public void DoWeGetOptionsBack() {
		B2Options result = B2Client.Authorize(Options);

		Assert.Multiple(() => {
			Assert.That(result.AbsoluteMinimumPartSize, Is.Not.EqualTo(0));
			Assert.That(result.MinimumPartSize, Is.Not.EqualTo(0));
			Assert.That(result.RecommendedPartSize, Is.Not.EqualTo(0));
			Assert.That(string.IsNullOrEmpty(result.DownloadUrl), Is.False);
			Assert.That(string.IsNullOrEmpty(result.ApiUrl), Is.False);
		});
	}
}
