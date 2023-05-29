using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

public class LargeFileTests : BaseTest {
	B2Client _client = null!;

	const string LARGE_FILE_NAME = "B2LargeFileTest.txt";

	static FileStream ReadLargeFile() => File.OpenRead(Path.Join("files", LARGE_FILE_NAME));

	[OneTimeSetUp]
	public void Setup() {
		_client = new B2Client(DefaultOptions);
	}

	// THIS TEST DOES NOT PROPERLY CLEAN UP after an exception.
	[Test]
	public async Task LargeFileUploadTest() {
		B2Bucket bucket = await CreateBucket();

		List<byte[]> parts = new();

		await using FileStream fileStream = ReadLargeFile();
		long fileSize = fileStream.Length;
		long totalBytesParted = 0;
		const long minPartSize = 1024 * 1024 * 5;

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

		B2File? start = null;
		B2File finish;
		try {
			start = _client.LargeFiles.StartLargeFile(LARGE_FILE_NAME, bucketId: bucket.BucketId).Result;

			for (int i = 0; i < parts.Count; i++) {
				B2UploadPartUrl uploadUrl = _client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				_ = _client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
			}

			finish = _client.LargeFiles.FinishLargeFile(start.FileId, parts.Select(Utils.GetSha1Hash).ToArray()).Result;
		} catch (Exception e) {
			if (start != null) {
				await _client.LargeFiles.CancelLargeFile(start.FileId);
			}

			Console.WriteLine(e);
			throw;
		}

		Assert.That(finish.FileId, Is.EqualTo(start.FileId), "File Ids did not match.");
	}

	[Test]
	public async Task LargeFileUploadIncompleteGetPartsTest() {
		B2Bucket bucket = await CreateBucket();

		await using FileStream fileStream = ReadLargeFile();
		using StreamReader stream = new(fileStream);
		List<byte[]> parts = new();
		const int partSize = 1024 * 1024 * 5;

		B2LargeFileParts listParts;

		while (stream.Peek() >= 0) {
			char[] c = new char[partSize];
			await stream.ReadAsync(c, 0, c.Length);
			parts.Add(Encoding.UTF8.GetBytes(c));
		}

		try {
			B2File start = _client.LargeFiles.StartLargeFile(LARGE_FILE_NAME, bucketId: bucket.BucketId).Result;

			for (int i = 0; i < 2; i++) {
				B2UploadPartUrl uploadUrl = _client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
				_ = _client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
			}

			// Now we can list parts and get a result
			listParts = _client.LargeFiles.ListPartsForIncompleteFile(start.FileId, 1, 100).Result;
		} catch (Exception e) {
			Console.WriteLine(e);
			throw;
		}

		Assert.That(listParts.Parts, Has.Count.EqualTo(2), "List of parts did not return expected amount of parts.");
	}

	[Test]
	public async Task LargeFileCancelTest() {
		B2Bucket bucket = await CreateBucket();

		await using FileStream fileStream = ReadLargeFile();
		using StreamReader stream = new(fileStream);
		List<byte[]> parts = new();
		const int partSize = 1024 * 1024 * 5;

		B2CancelledFile cancelledFile;

		while (stream.Peek() >= 0) {
			char[] c = new char[partSize];
			await stream.ReadAsync(c, 0, c.Length);

			parts.Add(Encoding.UTF8.GetBytes(c));
		}

		B2File start;
		try {
			start = await _client.LargeFiles.StartLargeFile(LARGE_FILE_NAME, bucketId: bucket.BucketId);

			for (int i = 0; i < 2; i++) {
				B2UploadPartUrl uploadUrl = await _client.LargeFiles.GetUploadPartUrl(start.FileId);
				_ = await _client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
			}

			// Now we can list parts and get a result
			cancelledFile = await _client.LargeFiles.CancelLargeFile(start.FileId);
		} catch (Exception e) {
			Console.WriteLine(e);
			throw;
		}

		Assert.That(cancelledFile.FileId, Is.EqualTo(start.FileId), "Started file and Cancelled file do not have the same id.");
	}

	[Test]
	public async Task LargeFileIncompleteListTest() {
		B2Bucket bucket = await CreateBucket();

		await using FileStream fileStream = ReadLargeFile();
		using StreamReader stream = new(fileStream);
		List<byte[]> parts = new();
		const int partSize = 1024 * 1024 * 5;

		B2IncompleteLargeFiles fileList;

		while (stream.Peek() >= 0) {
			char[] c = new char[partSize];
			await stream.ReadAsync(c, 0, c.Length);

			parts.Add(Encoding.UTF8.GetBytes(c));
		}

		B2File? start = null;
		try {
			start = await _client.LargeFiles.StartLargeFile(LARGE_FILE_NAME, bucketId: bucket.BucketId);

			for (int i = 0; i < 2; i++) {
				B2UploadPartUrl uploadUrl = await _client.LargeFiles.GetUploadPartUrl(start.FileId);
				_ = await _client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
			}

			// Now we can list parts and get a result
			fileList = await _client.LargeFiles.ListIncompleteFiles(bucket.BucketId);
		} catch (Exception e) {
			Console.WriteLine(e);
			throw;
		} finally {
			if (start != null) {
				_ = await _client.LargeFiles.CancelLargeFile(start.FileId);
			}
		}

		Assert.That(fileList.Files, Has.Count.EqualTo(1), "Incomplete file list count does not match what we expected.");
	}

	[Test]
	public async Task LargeFileCopyPartTest() {
		B2Bucket bucket = await CreateBucket();

		await using FileStream fileStream = ReadLargeFile();
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

		string[] shaHashes = parts.Select(Utils.GetSha1Hash).ToArray();

		B2File? start = null;
		B2File finish;
		List<B2UploadPart> uploadedParts = new();

		try {
			start = await _client.LargeFiles.StartLargeFile(LARGE_FILE_NAME, bucketId: bucket.BucketId);

			for (int i = 0; i < parts.Count; i++) {
				B2UploadPartUrl uploadUrl = await _client.LargeFiles.GetUploadPartUrl(start.FileId);
				uploadedParts.Add(await _client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl));
			}

			finish = await _client.LargeFiles.FinishLargeFile(start.FileId, shaHashes);
		} catch (Exception e) {
			if (start != null) {
				await _client.LargeFiles.CancelLargeFile(start.FileId);
			}

			Console.WriteLine(e);
			throw;
		}

		// Now we can copy the parts
		const string copyFileName = "B2LargeFileCopyTest.txt";
		B2File startCopy = await _client.LargeFiles.StartLargeFile(copyFileName, bucketId: bucket.BucketId);

		try {
			foreach (B2UploadPart unused in uploadedParts) {
				await _client.LargeFiles.CopyPart(finish.FileId, startCopy.FileId, 1, $"bytes=0-{minPartSize}");
			}

			B2File finishedCopy = await _client.LargeFiles.FinishLargeFile(startCopy.FileId, shaHashes);

			Assert.That(finishedCopy.ContentLength, Is.EqualTo(fileSize), "File sizes did not match.");
		} catch (Exception e) {
			await _client.LargeFiles.CancelLargeFile(startCopy.FileId);
			Console.WriteLine(e);
			throw;
		}
	}
}
