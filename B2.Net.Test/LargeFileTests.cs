using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using B2.Models;
using NUnit.Framework;

namespace B2.Test;

public class LargeFileTests : BaseTest {
	B2Bucket testBucket = new();
	B2Client client;
	readonly List<B2File> filesToDelete = new();
	string bucketName = "";

	static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../");

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

	// THIS TEST DOES NOT PROPERLY CLEAN UP after an exception.
	[Test]
	public async System.Threading.Tasks.Task LargeFileUploadTest() {
		const string fileName = "B2LargeFileTest.txt";
		FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
		List<byte[]> parts = new();
		long fileSize = fileStream.Length;
		long totalBytesParted = 0;
		const long minPartSize = 1024 * 5 * 1024;

		while (totalBytesParted < fileSize) {
			long partSize = minPartSize;
			// If last part is less than min part size, get that length
			if (fileSize - totalBytesParted < minPartSize) {
				partSize = fileSize - totalBytesParted;
			}

			byte[] c = new byte[partSize];
			fileStream.Seek(totalBytesParted, SeekOrigin.Begin);
			_ = fileStream.Read(c, 0, c.Length);

			parts.Add(c);
			totalBytesParted += partSize;
		}

		B2File start = null;
		B2File finish;
		try {
			start = client.LargeFiles.StartLargeFile(fileName, "", testBucket.BucketId).Result;

			for (int i = 0; i < parts.Count; i++) {
				B2UploadPartUrl uploadUrl = client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				_ = client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
			}

			finish = client.LargeFiles.FinishLargeFile(start.FileId, parts.Select(Utilities.GetSha1Hash).ToArray()).Result;
		} catch (Exception e) {
			if (start != null) {
				await client.LargeFiles.CancelLargeFile(start.FileId);
			}

			Console.WriteLine(e);
			throw;
		}

		// Clean up.
		filesToDelete.Add(start);


		Assert.That(finish.FileId, Is.EqualTo(start.FileId), "File Ids did not match.");
	}

	[Test]
	public void LargeFileUploadIncompleteGetPartsTest() {
		const string fileName = "B2LargeFileTest.txt";
		FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
		StreamReader stream = new(fileStream);
		List<byte[]> parts = new();

		B2LargeFileParts listParts;

		while (stream.Peek() >= 0) {
			char[] c = new char[1024 * (5 * 1024)];
			stream.Read(c, 0, c.Length);

			parts.Add(Encoding.UTF8.GetBytes(c));
		}

		B2File start = null;
		try {
			start = client.LargeFiles.StartLargeFile(fileName, "", testBucket.BucketId).Result;

			for (int i = 0; i < 2; i++) {
				B2UploadPartUrl uploadUrl = client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				_ = client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
			}

			// Now we can list parts and get a result
			listParts = client.LargeFiles.ListPartsForIncompleteFile(start.FileId, 1, 100).Result;
		} catch (Exception e) {
			Console.WriteLine(e);
			throw;
		} finally {
			// Clean up.
			filesToDelete.Add(start);
		}

		Assert.That(listParts.Parts, Has.Count.EqualTo(2), "List of parts did not return expected amount of parts.");
	}

	[Test]
	public void LargeFileCancelTest() {
		const string fileName = "B2LargeFileTest.txt";
		FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
		StreamReader stream = new(fileStream);
		List<byte[]> parts = new();

		B2CancelledFile cancelledFile;

		while (stream.Peek() >= 0) {
			char[] c = new char[1024 * 5 * 1024];
			stream.Read(c, 0, c.Length);

			parts.Add(Encoding.UTF8.GetBytes(c));
		}

		B2File start;
		try {
			start = client.LargeFiles.StartLargeFile(fileName, "", testBucket.BucketId).Result;

			for (int i = 0; i < 2; i++) {
				B2UploadPartUrl uploadUrl = client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				_ = client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
			}

			// Now we can list parts and get a result
			cancelledFile = client.LargeFiles.CancelLargeFile(start.FileId).Result;
		} catch (Exception e) {
			Console.WriteLine(e);
			throw;
		}

		Assert.That(cancelledFile.FileId, Is.EqualTo(start.FileId), "Started file and Cancelled file do not have the same id.");
	}

	[Test]
	public void LargeFileIncompleteListTest() {
		const string fileName = "B2LargeFileTest.txt";
		FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
		StreamReader stream = new(fileStream);
		List<byte[]> parts = new();

		B2IncompleteLargeFiles fileList;

		while (stream.Peek() >= 0) {
			char[] c = new char[1024 * (5 * 1024)];
			stream.Read(c, 0, c.Length);

			parts.Add(Encoding.UTF8.GetBytes(c));
		}

		B2File start = null;
		try {
			start = client.LargeFiles.StartLargeFile(fileName, "", testBucket.BucketId).Result;

			for (int i = 0; i < 2; i++) {
				B2UploadPartUrl uploadUrl = client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				_ = client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
			}

			// Now we can list parts and get a result
			fileList = client.LargeFiles.ListIncompleteFiles(testBucket.BucketId).Result;
		} catch (Exception e) {
			Console.WriteLine(e);
			throw;
		} finally {
			if (start != null) {
				_ = client.LargeFiles.CancelLargeFile(start.FileId).Result;
			}
		}

		Assert.That(fileList.Files, Has.Count.EqualTo(1), "Incomplete file list count does not match what we expected.");
	}

	[Test]
	public async System.Threading.Tasks.Task LargeFileCopyPartTest() {
		const string fileName = "B2LargeFileTest.txt";
		FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
		List<byte[]> parts = new();
		long fileSize = fileStream.Length;
		long totalBytesParted = 0;
		const long minPartSize = 1024 * 5 * 1024;

		while (totalBytesParted < fileSize) {
			long partSize = minPartSize;
			// If last part is less than min part size, get that length
			if (fileSize - totalBytesParted < minPartSize) {
				partSize = fileSize - totalBytesParted;
			}

			byte[] c = new byte[partSize];
			fileStream.Seek(totalBytesParted, SeekOrigin.Begin);
			_ = fileStream.Read(c, 0, c.Length);

			parts.Add(c);
			totalBytesParted += partSize;
		}

		string[] shaHashes = parts.Select(Utilities.GetSha1Hash).ToArray();

		B2File start = null;
		B2File finish;
		List<B2UploadPart> uploadedParts = new();

		try {
			start = client.LargeFiles.StartLargeFile(fileName, "", testBucket.BucketId).Result;

			for (int i = 0; i < parts.Count; i++) {
				B2UploadPartUrl uploadUrl = client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				uploadedParts.Add(client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result);
			}

			finish = client.LargeFiles.FinishLargeFile(start.FileId, shaHashes).Result;
		} catch (Exception e) {
			if (start != null) {
				await client.LargeFiles.CancelLargeFile(start.FileId);
			}

			Console.WriteLine(e);
			throw;
		}

		// Clean up.
		filesToDelete.Add(start);

		// Now we can copy the parts
		const string copyFileName = "B2LargeFileCopyTest.txt";
		B2File startCopy = client.LargeFiles.StartLargeFile(copyFileName, "", testBucket.BucketId).Result;

		try {
			foreach (B2UploadPart unused in uploadedParts) {
				await client.LargeFiles.CopyPart(finish.FileId, startCopy.FileId, 1, $"0-minPartSize.ToString()");
			}

			B2File finishedCopy = await client.LargeFiles.FinishLargeFile(startCopy.FileId, shaHashes);

			filesToDelete.Add(startCopy);

			Assert.That(finishedCopy.ContentLength, Is.EqualTo(fileSize.ToString()), "File sizes did not match.");
		} catch (Exception e) {
			await client.LargeFiles.CancelLargeFile(startCopy.FileId);
			Console.WriteLine(e);
			throw;
		}
	}

	[OneTimeTearDown]
	public void Cleanup() {
		foreach (B2File b2File in filesToDelete) {
			_ = client.Files.Delete(b2File.FileId, b2File.FileName).Result;
		}

		_ = client.Buckets.Delete(testBucket.BucketId).Result;
	}
}
