using System.Collections.Generic;
using System.Linq;
using B2.Models;

namespace B2.Test;

[SetUpFixture]
public class CleanupFixture {
	[OneTimeTearDown]
	public void OneTimeTearDown() {
		B2Client client = new(BaseTest.DefaultOptions);
		List<B2Bucket> list = client.Buckets.GetList().Result;

		foreach (B2Bucket bucket in list.Where(x => x.BucketName.Contains(BaseTest.TEST_BUCKET_PREFIX))) {
			B2FileList files = client.Files.GetList(bucketId: bucket.BucketId).Result;

			foreach (B2File file in files.Files) {
				B2FileList versions = client.Files.GetVersionsWithPrefixOrDelimiter(prefix: file.FileName, bucketId: bucket.BucketId).Result;
				
				foreach (B2File version in versions.Files) {
					client.Files.Delete(version.FileId, version.FileName).Wait();
				}
			}

			client.Buckets.Delete(bucket.BucketId).Wait();
		}
	}
}
