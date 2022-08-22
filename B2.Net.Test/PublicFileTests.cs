using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using B2.Models;

namespace B2.Test;

public class PublicFileTests : BaseTest {
	B2Bucket testBucket = new();
	B2Client client;
	readonly List<B2File> filesToDelete = new();
	const string BUCKET_NAME = "B2NETTestingBucketPublic";

	static string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");

	[OneTimeSetUp]
	public void Setup() {
		client = new B2Client(Options);
		Options = client.Authorize().Result;

		List<B2Bucket> buckets = client.Buckets.GetList().Result;
		B2Bucket existingBucket = null;

		foreach (B2Bucket b2Bucket in buckets.Where(b2Bucket => b2Bucket.BucketName == BUCKET_NAME)) {
			existingBucket = b2Bucket;
		}

		testBucket = existingBucket ?? client.Buckets.Create(BUCKET_NAME, BucketType.allPublic).Result;
	}

	[Test]
	public void FileGetFriendlyUrlTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utils.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		// Get url
		string friendlyUrl = client.Files.GetFriendlyDownloadUrl(fileName, testBucket.BucketName);

		// Test download
		HttpClient client2 = new();
		HttpResponseMessage friendFile = client2.GetAsync(friendlyUrl).Result;
		byte[] fileData2 = friendFile.Content.ReadAsByteArrayAsync().Result;
		string downloadHash = Utils.GetSha1Hash(fileData2);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}

	[OneTimeTearDown]
	public void Cleanup() {
		foreach (B2File b2File in filesToDelete) {
			_ = client.Files.Delete(b2File.FileId, b2File.FileName).Result;
		}

		_ = client.Buckets.Delete(testBucket.BucketId).Result;
	}
}
