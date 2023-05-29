using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

public class PublicFileTests : BaseTest {
	B2Client _client = null!;

	const string TEST_FILE_NAME = "B2Test.txt";
	
	static byte[] ReadTestFileBytes() => File.ReadAllBytes(Path.Join("files", TEST_FILE_NAME));
	
	[OneTimeSetUp]
	public void Setup() {
		_client = new B2Client(DefaultOptions);
	}

	[Test]
	public async Task FileGetFriendlyUrlTest() {
		B2Bucket bucket = await CreateBucket(isPublic: true);

		byte[] content = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(content);
		B2File file = await _client.Files.Upload(content, TEST_FILE_NAME, bucket.BucketId);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		// Get url
		string friendlyUrl = _client.Files.GetFriendlyDownloadUrl(file.FileName, bucket.BucketName);

		// Test download
		HttpClient client2 = new();
		HttpResponseMessage friendFile = await client2.GetAsync(friendlyUrl);
		byte[] fileData2 = await friendFile.Content.ReadAsByteArrayAsync();
		string downloadHash = Utils.GetSha1Hash(fileData2);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}
}
