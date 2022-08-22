using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

public class BucketTests : BaseTest {
	B2Client _client;
	readonly string bucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "")[..6]}";

	[OneTimeSetUp]
	public void Setup() {
		_client = new B2Client(B2Client.Authorize(Options));
	}

	[Test]
	public void GetBucketListTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, BucketType.allPrivate).Result;

		List<B2Bucket> list = _client.Buckets.GetList().Result;

		_ = _client.Buckets.Delete(bucket.BucketId).Result;

		Assert.That(list, Is.Not.Empty);
	}

	[Test]
	public void CreateBucketTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, BucketType.allPrivate).Result;

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}

		Assert.That(bucket.BucketName, Is.EqualTo(bucketName));
	}

	[Test]
	public void CreateBucketInvalidNameTest() {
		const string name = "B2net-testing-bucket-%$";

		Assert.Throws<AggregateException>(() => {
			_ = _client.Buckets.Create(name, BucketType.allPrivate).Result;
		});
	}

	[Test]
	public void CreateBucketWithCacheControlTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, new B2BucketOptions {
			CacheControl = 600
		}).Result;

		// Get bucket to check for info
		List<B2Bucket> bucketList = _client.Buckets.GetList().Result;

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}

		B2Bucket savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.That(savedBucket.BucketInfo, Is.Not.Null, "Bucket info was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket.BucketInfo.ContainsKey("cache-control"), "Bucket info did not contain Cache-Control");
			Assert.That(savedBucket.BucketInfo["cache-control"], Is.EqualTo("max-age=600"), "Cache-Control values were not equal.");
		});
	}

	[Test]
	public void CreateBucketWithLifecycleRulesTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, new B2BucketOptions {
			LifecycleRules = new List<B2BucketLifecycleRule> {
				new() {
					DaysFromHidingToDeleting = 30,
					DaysFromUploadingToHiding = null,
					FileNamePrefix = "testing"
				}
			}
		}).Result;

		// Get bucket to check for info
		List<B2Bucket> bucketList = _client.Buckets.GetList().Result;

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}

		B2Bucket savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket.BucketInfo, Is.Not.Null, "Bucket info was null");
			Assert.That(savedBucket.LifecycleRules, Has.Count.EqualTo(1), "Lifecycle rules count was " + savedBucket.LifecycleRules.Count);
		});
		Assert.Multiple(() => {
			Assert.That(savedBucket.LifecycleRules.First().FileNamePrefix, Is.EqualTo("testing"), "File name prefixes in the first lifecycle rule were not equal.");
			Assert.That(savedBucket.LifecycleRules.First().DaysFromUploadingToHiding, Is.EqualTo(null), "The first lifecycle rule DaysFromUploadingToHiding was not null");
			Assert.That(savedBucket.LifecycleRules.First().DaysFromHidingToDeleting, Is.EqualTo(30), "The first lifecycle rule DaysFromHidingToDeleting was not 30");
		});
	}

	[Test]
	public async Task UpdateBucketWithCacheControlTest() {
		B2Bucket bucket = await _client.Buckets.Create(bucketName, new B2BucketOptions { CacheControl = 600 });

		// Update bucket with new info
		bucket = await _client.Buckets.Update(new B2BucketOptions { CacheControl = 300 }, bucket.BucketId);

		// Get bucket to check for info
		List<B2Bucket> bucketList = await _client.Buckets.GetList();

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			await _client.Buckets.Delete(bucket.BucketId);
		}

		B2Bucket savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.That(savedBucket.BucketInfo, Is.Not.Null, "Bucket info was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket.BucketInfo.ContainsKey("cache-control"), "Bucket info did not contain Cache-Control");
			Assert.That(savedBucket.BucketInfo["cache-control"], Is.EqualTo("max-age=300"), "Cache-Control values were not equal.");
		});
	}

	[Test]
	public void UpdateBucketWithLifecycleRulesTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, new B2BucketOptions {
			LifecycleRules = new List<B2BucketLifecycleRule> {
				new() {
					DaysFromHidingToDeleting = 30,
					DaysFromUploadingToHiding = 15,
					FileNamePrefix = "testing"
				}
			}
		}).Result;

		// Update bucket with new info
		bucket = _client.Buckets.Update(new B2BucketOptions {
			LifecycleRules = new List<B2BucketLifecycleRule> {
				new() {
					DaysFromHidingToDeleting = 10,
					DaysFromUploadingToHiding = 10,
					FileNamePrefix = "tested"
				}
			}
		}, bucket.BucketId).Result;

		// Get bucket to check for info
		List<B2Bucket> bucketList = _client.Buckets.GetList().Result;

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}

		B2Bucket savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket.BucketInfo, Is.Not.Null, "Bucket info was null");
			Assert.That(savedBucket.LifecycleRules, Has.Count.EqualTo(1), "Lifecycle rules count was " + savedBucket.LifecycleRules.Count);
		});
		Assert.That(savedBucket.LifecycleRules.First().FileNamePrefix, Is.EqualTo("tested"), "File name prefixes in the first lifecycle rule were not equal.");
	}

	[Test]
	public void DeleteBucketTest() {
		//Creat a bucket to delete
		B2Bucket bucket = _client.Buckets.Create(bucketName, BucketType.allPrivate).Result;

		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			B2Bucket deletedBucket = _client.Buckets.Delete(bucket.BucketId).Result;
			Assert.That(deletedBucket.BucketName, Is.EqualTo(bucketName));
		} else {
			Assert.Fail("The bucket was not deleted. The response did not contain a bucketId.");
		}
	}

	[Test]
	public void UpdateBucketTest() {
		//Creat a bucket to delete
		B2Bucket bucket = _client.Buckets.Create(bucketName, BucketType.allPrivate).Result;

		try {
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				B2Bucket updatedBucket = _client.Buckets.Update(BucketType.allPublic, bucket.BucketId).Result;
				Assert.That(updatedBucket.BucketType, Is.EqualTo(BucketType.allPublic.ToString()));
			} else {
				Assert.Fail("The bucket was not deleted. The response did not contain a bucketId.");
			}
		} catch (Exception ex) {
			Assert.Fail(ex.Message);
		} finally {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}
	}

	[Test]
	public void BucketCorsRulesTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, new B2BucketOptions {
			CorsRules = new List<B2CorsRule> {
				new() {
					CorsRuleName = "allowAnyHttps",
					AllowedHeaders = new[] { "x-bz-content-sha1", "x-bz-info-*" },
					AllowedOperations = new[] { "b2_upload_file" },
					AllowedOrigins = new[] { "https://*" }
				}
			}
		}).Result;

		try {
			List<B2Bucket> list = _client.Buckets.GetList().Result;
			Assert.That(list, Is.Not.Empty);

			B2Bucket corsBucket = list.First(x => x.BucketId == bucket.BucketId);

			Assert.That(corsBucket.CorsRules.First().CorsRuleName, Is.EqualTo("allowAnyHttps"), "CORS header was not saved or returned for bucket.");
		} finally {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}
	}

	[Test]
	public void BucketCorsRuleUpdateTest() {
		B2Bucket bucket = _client.Buckets.Create(bucketName, new B2BucketOptions {
			CorsRules = new List<B2CorsRule> {
				new() {
					CorsRuleName = "allowAnyHttps",
					AllowedHeaders = new[] { "x-bz-content-sha1", "x-bz-info-*" },
					AllowedOperations = new[] { "b2_upload_file" },
					AllowedOrigins = new[] { "https://*" }
				}
			}
		}).Result;

		try {
			_ = _client.Buckets.Update(new B2BucketOptions {
				CorsRules = new List<B2CorsRule> {
					new() {
						CorsRuleName = "updatedRule",
						AllowedOperations = new[] { "b2_upload_part" },
						AllowedOrigins = new[] { "https://*" }
					}
				}
			}, bucket.Revision, bucket.BucketId).Result;

			List<B2Bucket> list = _client.Buckets.GetList().Result;
			Assert.That(list, Is.Not.Empty);

			B2Bucket corsBucket = list.First(x => x.BucketId == bucket.BucketId);

			Assert.Multiple(() => {
				Assert.That(corsBucket.CorsRules.First().CorsRuleName, Is.EqualTo("updatedRule"), "CORS header was not updated for bucket.");
				Assert.That(corsBucket.CorsRules.First().AllowedOperations.First(), Is.EqualTo("b2_upload_part"), "CORS header was not updated for bucket.");
			});
		} finally {
			_client.Buckets.Delete(bucket.BucketId).Wait();
		}
	}

	[Test]
	public async Task CleanUpAccount() {
		// Only use this test to clean up an account after tests run if buckets are left over.
		List<B2Bucket> list = await _client.Buckets.GetList();

		foreach (B2Bucket b2Bucket in list.Where(x => x.BucketName.Contains("B2NETTestingBucket"))) {
			B2FileList files = await _client.Files.GetList(bucketId: b2Bucket.BucketId);

			if (files.Files.Count > 0) {
				foreach (B2File file in files.Files) {
					await _client.Files.Delete(file.FileId, file.FileName);
				}
			}

			await _client.Buckets.Delete(b2Bucket.BucketId);
		}
	}
}
