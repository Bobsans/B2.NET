using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

public class FileTests : BaseTest {
	B2Client _client = null!;

	const string TEST_FILE_NAME = "B2Test.txt";

	static byte[]? _content;
	static byte[] ReadTestFileBytes() => _content ??= File.ReadAllBytes(Path.Join("files", TEST_FILE_NAME));

	async Task<B2File> UploadTestFile(B2Bucket bucket, string? name = null, byte[]? content = null) {
		return await _client.Files.Upload(content ?? ReadTestFileBytes(), name ?? TEST_FILE_NAME, bucket.BucketId);
	}

	async Task<(string, B2File)> UploadTestFileWithSha(B2Bucket bucket, byte[]? content = null) {
		content ??= ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(content);
		B2File file = await _client.Files.Upload(content, TEST_FILE_NAME, bucket.BucketId);
		return (hash, file);
	}
	
	[OneTimeSetUp]
	public void Setup() {
		_client = new B2Client(DefaultOptions);
	}

	[Test]
	public async Task GetListTest() {
		B2Bucket bucket = await CreateBucket();

		await UploadTestFile(bucket);

		B2FileList list = await _client.Files.GetList(bucketId: bucket.BucketId);

		Assert.That(list.Files, Has.Count.EqualTo(1), $"{list.Files.Count} files found.");
	}

	[Test]
	public async Task GetListWithPrefixTest() {
		B2Bucket bucket = await CreateBucket();

		const string fileNameWithFolder = "test/B2Test.txt";

		await UploadTestFile(bucket);
		await UploadTestFile(bucket, fileNameWithFolder);

		B2FileList list = await _client.Files.GetListWithPrefixOrDelimiter(bucketId: bucket.BucketId, prefix: "test");

		Assert.That(list.Files, Has.Count.EqualTo(1), $"{list.Files.Count} files found.");
	}

	[Test]
	public async Task GetListWithPrefixAndDelimiterTest() {
		B2Bucket bucket = await CreateBucket();

		const string fileNameWithFolder = "test/B2Test.txt";

		await UploadTestFile(bucket);
		await UploadTestFile(bucket, fileNameWithFolder);

		B2FileList list = await _client.Files.GetListWithPrefixOrDelimiter(bucketId: bucket.BucketId, prefix: "test", delimiter: "/");

		Assert.That(list.Files, Has.Count.EqualTo(1), $"{list.Files.Count} files found.");
		Assert.That(list.Files.First().FileName, Is.EqualTo("test/"), "File names to not match.");
	}

	[Test]
	public async Task GetListWithDelimiterTest() {
		B2Bucket bucket = await CreateBucket();

		const string fileNameWithFolder = "test/B2Test.txt";
		const string fileNameWithFolder2 = "test2/B2Test.txt";

		await UploadTestFile(bucket, fileNameWithFolder);
		await UploadTestFile(bucket, fileNameWithFolder2);

		List<B2File> list = _client.Files.GetListWithPrefixOrDelimiter(bucketId: bucket.BucketId, delimiter: "/").Result.Files;

		Assert.That(list, Has.Count.EqualTo(2), $"{list.Count} files found.");
		Assert.That(list.All(f => f.Action == "folder"), "Not all list items were folders.");
	}

	// [Test]
	// public void EmptyBucket() {
	// 	List<B2File> list = _client.Files.GetList(bucketId: testBucket.BucketId).Result.Files;
	//
	// 	foreach (B2File b2File in list) {
	// 		_ = _client.Files.Delete(b2File.FileId, b2File.FileName).Result;
	// 	}
	// }

	// [Test]
	// public void HideFileTest() {
	// 	const string fileName = "B2Test.txt";
	// 	byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
	// 	string hash = Utils.GetSha1Hash(fileData);
	// 	B2File file = _client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
	// 	// Clean up.
	// 	filesToDelete.Add(file);
	//
	// 	Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");
	//
	// 	B2File hiddenFile = _client.Files.Hide(file.FileName, testBucket.BucketId).Result;
	//
	// 	Assert.That(hiddenFile.Action, Is.EqualTo("hide"));
	//
	// 	// Cancel hide file so we can delete it later
	// 	_ = _client.Files.Hide(file.FileName, testBucket.BucketId).Result;
	// }

	[Test]
	public async Task FileUploadTest() {
		B2Bucket bucket = await CreateBucket();

		(string hash, B2File file) result = await UploadTestFileWithSha(bucket);

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");
	}

	[Test]
	public async Task FileUploadUsingUploadUrlTest() {
		B2Bucket bucket = await CreateBucket();

		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		B2UploadUrl uploadUrl = await _client.Files.GetUploadUrl(bucket.BucketId);
		B2File file = await _client.Files.Upload(fileData, TEST_FILE_NAME, uploadUrl, true, bucket.BucketId);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");
	}

	[Test]
	public async Task FileUploadWithInfoTest() {
		B2Bucket bucket = await CreateBucket();

		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = await _client.Files.Upload(fileData, TEST_FILE_NAME, bucket.BucketId, fileInfo);

		Assert.Multiple(() => {
			Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");
			Assert.That(file.FileInfo, Has.Count.EqualTo(1), "File info count was off.");
		});
	}

	[Test]
	public async Task FileUploadStreamTest() {
		B2Bucket bucket = await CreateBucket();

		byte[] bytes = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(bytes);
		byte[] hashBytes = Encoding.UTF8.GetBytes(hash);

		MemoryStream fileData = new(bytes.Concat(hashBytes).ToArray());

		B2UploadUrl uploadUrl = await _client.Files.GetUploadUrl(bucket.BucketId);
		B2File file = await _client.Files.Upload(fileData, TEST_FILE_NAME, uploadUrl, null, false, bucket.BucketId);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");
	}

	[Test]
	public async Task FileUploadStreamNoShaTest() {
		B2Bucket bucket = await CreateBucket();

		byte[] bytes = ReadTestFileBytes();

		MemoryStream fileData = new(bytes);
		B2UploadUrl uploadUrl = await _client.Files.GetUploadUrl(bucket.BucketId);
		B2File file = await _client.Files.Upload(fileData, TEST_FILE_NAME, uploadUrl, bucketId: bucket.BucketId, dontSha: true);

		Assert.That(file.ContentSha1, Does.StartWith("unverified"), $"File was verified when it should not have been: {file.ContentSha1}.");
	}

	[Test]
	public async Task FileDownloadNameTest() {
		B2Bucket bucket = await CreateBucket();

		(string hash, B2File file) result = await UploadTestFileWithSha(bucket);

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		// Test download
		B2File download = await _client.Files.DownloadByName(result.file.FileName, bucket.BucketName);
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(result.hash));
	}

	[Test]
	public async Task FileDownloadWithInfoTest() {
		B2Bucket bucket = await CreateBucket();

		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = await _client.Files.Upload(fileData, TEST_FILE_NAME, bucket.BucketId, fileInfo);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = await _client.Files.DownloadById(file.FileId);
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.Multiple(() => {
			Assert.That(downloadHash, Is.EqualTo(hash));
			Assert.That(download.FileInfo, Has.Count.EqualTo(1));
		});
	}

	[Test]
	public async Task FileDownloadIdTest() {
		B2Bucket bucket = await CreateBucket();
		
		(string hash, B2File file) result = await UploadTestFileWithSha(bucket);

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		// Test download
		B2File download = await _client.Files.DownloadById(result.file.FileId);
		
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(result.hash));
	}

	[Test]
	public async Task FileDownloadFolderTest() {
		B2Bucket bucket = await CreateBucket();
		
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);
		
		B2File file = await _client.Files.Upload(fileData, "B2Folder/Test/File.txt", bucket.BucketId);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = await _client.Files.DownloadById(file.FileId);
		
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}

	[Test]
	public async Task FileDeleteTest() {
		B2Bucket bucket = await CreateBucket();
		
		(string hash, B2File file) result = await UploadTestFileWithSha(bucket);

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		// Clean up. We have to delete the file before we can delete the bucket
		B2File deletedFile = await _client.Files.Delete(result.file.FileId, result.file.FileName);

		Assert.That(deletedFile.FileId, Is.EqualTo(result.file.FileId), "The deleted file id did not match.");
	}

	[Test]
	public async Task ListVersionsTest() {
		B2Bucket bucket = await CreateBucket();
		
		(string hash, B2File file) result = await UploadTestFileWithSha(bucket);

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		B2FileList versions = await _client.Files.GetVersions(result.file.FileName, result.file.FileId, bucketId: bucket.BucketId);

		Assert.That(versions.Files, Has.Count.EqualTo(1));
	}

	[Test]
	public async Task GetInfoTest() {
		B2Bucket bucket = await CreateBucket();
		
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = await _client.Files.Upload(fileData, TEST_FILE_NAME, bucket.BucketId, fileInfo);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		B2File info = await _client.Files.GetInfo(file.FileId);

		Assert.Multiple(() => {
			Assert.That(info.UploadTimestamp, Is.EqualTo(file.UploadTimestamp));
			Assert.That(info.FileInfo, Has.Count.EqualTo(1));
		});
	}

	[Test]
	public async Task GetDownloadAuthorizationTest() {
		B2Bucket bucket = await CreateBucket();
		
		B2DownloadAuthorization downloadAuth = await _client.Files.GetDownloadAuthorization("Test", 120, bucket.BucketId);

		Assert.That(downloadAuth.FileNamePrefix, Is.EqualTo("Test"), "File prefixes were not the same.");
	}

	[Test]
	public async Task CopyFile() {
		B2Bucket bucket = await CreateBucket();
		
		byte[] content = ReadTestFileBytes();
		B2File file = await UploadTestFile(bucket, content: content);

		B2File copied = await _client.Files.Copy(file.FileId, "B2TestCopy.txt");

		Assert.Multiple(() => {
			Assert.That(copied.Action, Is.EqualTo("copy"), "Action was not as expected for the copy operation.");
			Assert.That(copied.ContentLength, Is.EqualTo(content.Length), "Length of the two files was not the same.");
		});
	}

	[Test]
	public async Task ReplaceFile() {
		B2Bucket bucket = await CreateBucket();
		
		byte[] content = ReadTestFileBytes();
		B2File file = await UploadTestFile(bucket, content:content);

		B2File copied = await _client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE, "text/plain", new Dictionary<string, string> {
			{ "FileInfoTest", "1234" }
		});

		Assert.Multiple(() => {
			Assert.That(copied.FileInfo.ContainsKey("fileinfotest"), "FileInfo was not as expected for the replace operation.");
			Assert.That(copied.ContentLength, Is.EqualTo(content.Length), "Length of the two files was not the same.");
		});
	}

	[Test]
	public async Task CopyFileWithDisallowedFields() {
		B2Bucket bucket = await CreateBucket();
		
		B2File file = await UploadTestFile(bucket);

		Assert.ThrowsAsync<CopyReplaceSetupException>(async () => {
			await _client.Files.Copy(file.FileId, "B2TestCopy.txt", contentType: "b2/x-auto");
		}, "Copy did not fail when disallowed fields were provided.");
	}

	[Test]
	public async Task ReplaceFileWithMissingFields() {
		B2Bucket bucket = await CreateBucket();
		
		B2File file = await UploadTestFile(bucket);

		Assert.ThrowsAsync<CopyReplaceSetupException>(async () => {
			await _client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE);
		}, "Replace did not fail when fields were missing.");
	}
}
