using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2.Models;
using NUnit.Framework;

namespace B2.Test;

public class FileTests : BaseTest {
	B2Bucket testBucket = new();
	B2Client client;
	readonly List<B2File> filesToDelete = new();
	string bucketName = "";

	static string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");

	[OneTimeSetUp]
	public void Setup() {
		client = new B2Client(Options);
		bucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";

		List<B2Bucket> buckets = client.Buckets.GetList().Result;
		B2Bucket existingBucket = null;
		foreach (B2Bucket b2Bucket in buckets.Where(b2Bucket => b2Bucket.BucketName == bucketName)) {
			existingBucket = b2Bucket;
		}

		testBucket = existingBucket ?? client.Buckets.Create(bucketName, BucketTypes.allPrivate).Result;
	}

	[Test]
	public void GetListTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		List<B2File> list = client.Files.GetList(bucketId: testBucket.BucketId).Result.Files;

		Assert.That(list, Has.Count.EqualTo(1), list.Count + " files found.");
	}

	[Test]
	public void GetListWithPrefixTest() {
		const string fileName = "B2Test.txt";
		const string fileNameWithFolder = "test/B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		B2File fileFolder = client.Files.Upload(fileData, fileNameWithFolder, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);
		filesToDelete.Add(fileFolder);

		List<B2File> list = client.Files.GetListWithPrefixOrDelimiter(bucketId: testBucket.BucketId, prefix: "test").Result.Files;

		Assert.That(list, Has.Count.EqualTo(1), list.Count + " files found.");
	}

	[Test]
	public void GetListWithPrefixAndDelimiterTest() {
		const string fileName = "B2Test.txt";
		const string fileNameWithFolder = "test/B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		B2File fileFolder = client.Files.Upload(fileData, fileNameWithFolder, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);
		filesToDelete.Add(fileFolder);

		List<B2File> list = client.Files.GetListWithPrefixOrDelimiter(bucketId: testBucket.BucketId, prefix: "test", delimiter: "/").Result.Files;

		Assert.That(list, Has.Count.EqualTo(1), list.Count + " files found.");
		Assert.That(list.First().FileName, Is.EqualTo("test/"), "File names to not match.");
	}

	[Test]
	public void GetListWithDelimiterTest() {
		const string fileName = "B2Test.txt";
		const string fileNameWithFolder = "test/B2Test.txt";
		const string fileNameWithFolder2 = "test2/B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileNameWithFolder2, testBucket.BucketId).Result;
		B2File fileFolder = client.Files.Upload(fileData, fileNameWithFolder, testBucket.BucketId).Result;
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
	// 	string hash = Utilities.GetSha1Hash(fileData);
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
	public void FileUploadTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");
	}

	[Test]
	public void FileUploadUsingUploadUrlTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);

		B2UploadUrl uploadUrl = client.Files.GetUploadUrl(testBucket.BucketId).Result;

		B2File file = client.Files.Upload(fileData, fileName, uploadUrl, true, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");
	}

	//[Test]
	//public void FileUploadEncodingTest() {
	//	var fileName = "B2 Test File.txt";
	//	var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
	//	string hash = Utilities.GetSHA1Hash(fileData);
	//	var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

	//	// Clean up.
	//	FilesToDelete.Add(file);

	//	Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
	//}

	[Test]
	public void FileUploadWithInfoTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId, fileInfo).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.Multiple(() => {
			Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");
			Assert.That(file.FileInfo, Has.Count.EqualTo(1), "File info count was off.");
		});
	}

	[Test]
	public void FileUploadStreamTest() {
		const string fileName = "B2Test.txt";
		byte[] bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

		string hash = Utilities.GetSha1Hash(bytes);
		byte[] hashBytes = Encoding.UTF8.GetBytes(hash);

		MemoryStream fileData = new(bytes.Concat(hashBytes).ToArray());

		B2UploadUrl uploadUrl = client.Files.GetUploadUrl(testBucket.BucketId).Result;

		B2File file = client.Files.Upload(fileData, fileName, uploadUrl, "", false, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");
	}

	[Test]
	public void FileUploadStreamNoShaTest() {
		const string fileName = "B2Test.txt";
		byte[] bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

		MemoryStream fileData = new(bytes);

		B2UploadUrl uploadUrl = client.Files.GetUploadUrl(testBucket.BucketId).Result;

		B2File file = client.Files.Upload(fileData, fileName, uploadUrl, "", false, testBucket.BucketId, null, true).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Does.StartWith("unverified"), $"File was verified when it should not have been: {file.ContentSHA1}.");
	}

	[Test]
	public void FileDownloadNameTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadByName(file.FileName, testBucket.BucketName).Result;
		string downloadHash = Utilities.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}

	[Test]
	public void FileDownloadWithInfoTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId, fileInfo).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadById(file.FileId).Result;
		string downloadHash = Utilities.GetSha1Hash(download.FileData);

		Assert.Multiple(() => {
			Assert.That(downloadHash, Is.EqualTo(hash));
			Assert.That(download.FileInfo, Has.Count.EqualTo(1));
		});
	}

	[Test]
	public void FileDownloadIdTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadById(file.FileId).Result;
		string downloadHash = Utilities.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}

	[Test]
	public void FileDownloadFolderTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, "B2Folder/Test/File.txt", testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

		// Test download
		B2File download = client.Files.DownloadById(file.FileId).Result;
		string downloadHash = Utilities.GetSha1Hash(download.FileData);

		Assert.That(downloadHash, Is.EqualTo(hash));
	}

	[Test]
	public void FileDeleteTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

		// Clean up. We have to delete the file before we can delete the bucket
		B2File deletedFile = client.Files.Delete(file.FileId, file.FileName).Result;

		Assert.That(deletedFile.FileId, Is.EqualTo(file.FileId), "The deleted file id did not match.");
	}

	[Test]
	public void ListVersionsTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

		B2FileList versions = client.Files.GetVersions(file.FileName, file.FileId, bucketId: testBucket.BucketId).Result;

		Assert.That(versions.Files, Has.Count.EqualTo(1));
	}

	[Test]
	public void GetInfoTest() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		string hash = Utilities.GetSha1Hash(fileData);

		Dictionary<string, string> fileInfo = new() {
			{ "FileInfoTest", "1234" }
		};

		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId, fileInfo).Result;
		// Clean up.
		filesToDelete.Add(file);

		Assert.That(file.ContentSHA1, Is.EqualTo(hash), "File hashes did not match.");

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
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		B2File copied = await client.Files.Copy(file.FileId, "B2TestCopy.txt");
		// Clean up.
		filesToDelete.Add(copied);

		Assert.Multiple(() => {
			Assert.That(copied.Action, Is.EqualTo("copy"), "Action was not as expected for the copy operation.");
			Assert.That(copied.ContentLength, Is.EqualTo(fileData.Length.ToString()), "Length of the two files was not the same.");
		});
	}

	[Test]
	public async Task ReplaceFile() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		// Clean up.
		filesToDelete.Add(file);

		B2File copied = await client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE, "text/plain", new Dictionary<string, string> {
			{ "FileInfoTest", "1234" }
		});
		// Clean up.
		filesToDelete.Add(copied);

		Assert.Multiple(() => {
			Assert.That(copied.FileInfo.ContainsKey("fileinfotest"), "FileInfo was not as expected for the replace operation.");
			Assert.That(copied.ContentLength, Is.EqualTo(fileData.Length.ToString()), "Length of the two files was not the same.");
		});
	}

	[Test]
	public void CopyFileWithDisallowedFields() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;

		// Clean up.
		filesToDelete.Add(file);

		Assert.ThrowsAsync<CopyReplaceSetupException>(async () => {
			await client.Files.Copy(file.FileId, "B2TestCopy.txt", contentType: "b2/x-auto");
		}, "Copy did not fail when disallowed fields were provided.");
	}

	[Test]
	public void ReplaceFileWithMissingFields() {
		const string fileName = "B2Test.txt";
		byte[] fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
		B2File file = client.Files.Upload(fileData, fileName, testBucket.BucketId).Result;
		
		// Clean up.
		filesToDelete.Add(file);

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
