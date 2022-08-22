using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

public class FileTests : BaseTest {
	B2Bucket testBucket = new();
	B2Client client;
	readonly List<B2File> filesToDelete = new();
	string bucketName = "";

	const string TEST_FILE_NAME = "B2Test.txt";

	static byte[] ReadTestFileBytes() => File.ReadAllBytes(Path.Join("files", TEST_FILE_NAME));

	async Task<B2File> UploadTestFile(byte[] content = null) {
		B2File file = await client.Files.Upload(content ?? ReadTestFileBytes(), TEST_FILE_NAME, testBucket.BucketId);
		filesToDelete.Add(file);
		return file;
	}

	async Task<(string, B2File)> UploadTestFileWithSha(byte[] content = null) {
		content ??= ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(content);
		B2File file = await client.Files.Upload(content, TEST_FILE_NAME, testBucket.BucketId);
		filesToDelete.Add(file);
		return (hash, file);
	}

	[OneTimeSetUp]
	public void Setup() {
		client = new B2Client(B2Client.Authorize(Options));
		bucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "")[..6]}";

		List<B2Bucket> buckets = client.Buckets.GetList().Result;
		B2Bucket existingBucket = null;
		foreach (B2Bucket b2Bucket in buckets.Where(b2Bucket => b2Bucket.BucketName == bucketName)) {
			existingBucket = b2Bucket;
		}

		testBucket = existingBucket ?? client.Buckets.Create(bucketName, BucketType.allPrivate).Result;
	}

	[Test]
	public async Task GetListTest() {
		await UploadTestFile();

		B2FileList list = await client.Files.GetList(bucketId: testBucket.BucketId);

		Assert.That(list.Files, Has.Count.EqualTo(1), list.Files.Count + " files found.");
	}

	[Test]
	public void GetListWithPrefixTest() {
		const string fileNameWithFolder = "test/B2Test.txt";
		byte[] fileData = ReadTestFileBytes();
		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, testBucket.BucketId).Result;
		B2File fileFolder = client.Files.Upload(fileData, fileNameWithFolder, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);
		filesToDelete.Add(fileFolder);

		List<B2File> list = client.Files.GetListWithPrefixOrDelimiter(bucketId: testBucket.BucketId, prefix: "test").Result.Files;

		Assert.That(list, Has.Count.EqualTo(1), list.Count + " files found.");
	}

	[Test]
	public async Task GetListWithPrefixAndDelimiterTest() {
		const string fileNameWithFolder = "test/B2Test.txt";
		byte[] content = ReadTestFileBytes();

		B2File file = client.Files.Upload(content, TEST_FILE_NAME, testBucket.BucketId).Result;
		B2File fileFolder = client.Files.Upload(content, fileNameWithFolder, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);
		filesToDelete.Add(fileFolder);

		B2FileList list = await client.Files.GetListWithPrefixOrDelimiter(bucketId: testBucket.BucketId, prefix: "test", delimiter: "/");

		Assert.That(list.Files, Has.Count.EqualTo(1), list.Files.Count + " files found.");
		Assert.That(list.Files.First().FileName, Is.EqualTo("test/"), "File names to not match.");
	}

	[Test]
	public void GetListWithDelimiterTest() {
		const string fileNameWithFolder = "test/B2Test.txt";
		const string fileNameWithFolder2 = "test2/B2Test.txt";

		byte[] content = ReadTestFileBytes();
		B2File file = client.Files.Upload(content, fileNameWithFolder2, testBucket.BucketId).Result;
		B2File fileFolder = client.Files.Upload(content, fileNameWithFolder, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);
		filesToDelete.Add(fileFolder);

		List<B2File> list = client.Files.GetListWithPrefixOrDelimiter(bucketId: testBucket.BucketId, delimiter: "/").Result.Files;

		Assert.That(list, Has.Count.EqualTo(2), list.Count + " files found.");
		Assert.That(list.All(f => f.Action == "folder"), "Not all list items were folders.");
	}

	// [Test]
	// public void EmptyBucket() {
	// 	List<B2File> list = client.Files.GetList(bucketId: testBucket.BucketId).Result.Files;
	//
	// 	foreach (B2File b2File in list) {
	// 		_ = client.Files.Delete(b2File.FileId, b2File.FileName).Result;
	// 	}
	// }

	// [Test]
	// public void HideFileTest() {
	// 	const string fileName = "B2Test.txt";
	// 	byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
	// 	string hash = Utils.GetSha1Hash(fileData);
	// 	B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
	// 	// Clean up.
	// 	filesToDelete.Add(file);
	//
	// 	Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");
	//
	// 	B2File hiddenFile = client.Files.Hide(file.FileName, testBucket.BucketId).Result;
	//
	// 	Assert.That(hiddenFile.Action, Is.EqualTo("hide"));
	//
	// 	// Cancel hide file so we can delete it later
	// 	_ = client.Files.Hide(file.FileName, testBucket.BucketId).Result;
	// }

	[Test]
	public async Task FileUploadTest() {
		(string hash, B2File file) result = await UploadTestFileWithSha();

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");
	}

	[Test]
	public void FileUploadUsingUploadUrlTest() {
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		B2UploadUrl uploadUrl = client.Files.GetUploadUrl(testBucket.BucketId).Result;

		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, uploadUrl, true, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");
	}

	[Test]
	public void FileUploadWithInfoTest() {
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, testBucket.BucketId, fileInfo).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.Multiple(() => {
			Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");
			Assert.That(file.FileInfo, Has.Count.EqualTo(1), "File info count was off.");
		});
	}

	[Test]
	public void FileUploadStreamTest() {
		byte[] bytes = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(bytes);
		byte[] hashBytes = Encoding.UTF8.GetBytes(hash);

		MemoryStream fileData = new(bytes.Concat(hashBytes).ToArray());

		B2UploadUrl uploadUrl = client.Files.GetUploadUrl(testBucket.BucketId).Result;

		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, uploadUrl, null, false, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");
	}

	[Test]
	public void FileUploadStreamNoShaTest() {
		byte[] bytes = ReadTestFileBytes();

		MemoryStream fileData = new(bytes);
		B2UploadUrl uploadUrl = client.Files.GetUploadUrl(testBucket.BucketId).Result;
		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, uploadUrl, bucketId: testBucket.BucketId, dontSha: true).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Does.StartWith("unverified"), $"File was verified when it should not have been: {file.ContentSha1}.");
	}

	[Test]
	public async Task FileDownloadNameTest() {
		(string hash, B2File file) result = await UploadTestFileWithSha();

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadByName(result.file.FileName, testBucket.BucketName).Result;
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(result.hash));
	}

	[Test]
	public void FileDownloadWithInfoTest() {
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, testBucket.BucketId, fileInfo).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadById(file.FileId).Result;
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.Multiple(() => {
			Assert.That(downloadHash, Is.EqualTo(hash));
			Assert.That(download.FileInfo, Has.Count.EqualTo(1));
		});
	}

	[Test]
	public async Task FileDownloadIdTest() {
		(string hash, B2File file) result = await UploadTestFileWithSha();

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		// Test download
		B2File download = await client.Files.DownloadById(result.file.FileId);
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(result.hash));
	}

	[Test]
	public void FileDownloadFolderTest() {
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, "B2Folder/Test/File.txt", testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadById(file.FileId).Result;
		string downloadHash = Utils.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}

	[Test]
	public async Task FileDeleteTest() {
		(string hash, B2File file) result = await UploadTestFileWithSha();

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		// Clean up. We have to delete the file before we can delete the bucket
		B2File deletedFile = client.Files.Delete(result.file.FileId, result.file.FileName).Result;

		Assert.That(deletedFile.FileId, Is.EqualTo(result.file.FileId), "The deleted file id did not match.");
	}

	[Test]
	public async Task ListVersionsTest() {
		(string hash, B2File file) result = await UploadTestFileWithSha();

		Assert.That(result.file.ContentSha1, Is.EqualTo(result.hash), "File hashes did not match.");

		B2FileList versions = client.Files.GetVersions(result.file.FileName, result.file.FileId, bucketId: testBucket.BucketId).Result;

		Assert.That(versions.Files, Has.Count.EqualTo(1));
	}

	[Test]
	public void GetInfoTest() {
		byte[] fileData = ReadTestFileBytes();
		string hash = Utils.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = client.Files.Upload(fileData, TEST_FILE_NAME, testBucket.BucketId, fileInfo).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSha1, Is.EqualTo(hash), "File hashes did not match.");

		B2File info = client.Files.GetInfo(file.FileId).Result;

		Assert.Multiple(() => {
			Assert.That(info.UploadTimestamp, Is.EqualTo(file.UploadTimestamp));
			Assert.That(info.FileInfo, Has.Count.EqualTo(1));
		});
	}

	[Test]
	public void GetDownloadAuthorizationTest() {
		B2DownloadAuthorization downloadAuth = client.Files.GetDownloadAuthorization("Test", 120, testBucket.BucketId).Result;

		Assert.That(downloadAuth.FileNamePrefix, Is.EqualTo("Test"), "File prefixes were not the same.");
	}

	[Test]
	public async Task CopyFile() {
		byte[] content = ReadTestFileBytes();
		B2File file = await UploadTestFile(content);

		B2File copied = await client.Files.Copy(file.FileId, "B2TestCopy.txt");

		// Clean up.
		filesToDelete.Add(copied);

		Assert.Multiple(() => {
			Assert.That(copied.Action, Is.EqualTo("copy"), "Action was not as expected for the copy operation.");
			Assert.That(copied.ContentLength, Is.EqualTo(content.Length), "Length of the two files was not the same.");
		});
	}

	[Test]
	public async Task ReplaceFile() {
		byte[] content = ReadTestFileBytes();
		B2File file = await UploadTestFile(content);

		B2File copied = await client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE, "text/plain", new Dictionary<string, string> {
			{ "FileInfoTest", "1234" }
		});

		// Clean up.
		filesToDelete.Add(copied);

		Assert.Multiple(() => {
			Assert.That(copied.FileInfo.ContainsKey("fileinfotest"), "FileInfo was not as expected for the replace operation.");
			Assert.That(copied.ContentLength, Is.EqualTo(content.Length), "Length of the two files was not the same.");
		});
	}

	[Test]
	public async Task CopyFileWithDisallowedFields() {
		B2File file = await UploadTestFile();

		Assert.ThrowsAsync<CopyReplaceSetupException>(async () => {
			await client.Files.Copy(file.FileId, "B2TestCopy.txt", contentType: "b2/x-auto");
		}, "Copy did not fail when disallowed fields were provided.");
	}

	[Test]
	public async Task ReplaceFileWithMissingFields() {
		B2File file = await UploadTestFile();

		Assert.ThrowsAsync<CopyReplaceSetupException>(async () => {
			await client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE);
		}, "Replace did not fail when fields were missing.");
	}

	[OneTimeTearDown]
	public void Cleanup() {
		foreach (B2File b2File in filesToDelete) {
			_ = client.Files.Delete(b2File.FileId, b2File.FileName).Result;
		}

		_ = client.Buckets.Delete(testBucket.BucketId).Result;
	}
}
