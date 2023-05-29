using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2.Models;

namespace B2.Test;

public class BucketTests : BaseTest {
	B2Client _client = null!;

	[OneTimeSetUp]
	public void Setup() {
		_client = new B2Client(DefaultOptions);
	}

	[Test]
	public async Task GetBucketListTest() {
		await CreateBucket();

		List<B2Bucket> list = await _client.Buckets.GetList();

		Assert.That(list, Is.Not.Empty);
	}

	[Test]
	public async Task CreateBucketTest() {
		string name = GetNewBucketName();
		B2Bucket bucket = await CreateBucket(name);

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			await _client.Buckets.Delete(bucket.BucketId);
		}

		Assert.That(bucket.BucketName, Is.EqualTo(name));
	}

	[Test]
	public void CreateBucketInvalidNameTest() {
		const string name = "B2net-testing-bucket-%$";

		Assert.Throws<AggregateException>(() => {
			new B2Client(DefaultOptions).Buckets.Create(name, BucketType.AllPrivate).Wait();
		});
	}

	[Test]
	public async Task CreateBucketWithCacheControlTest() {
		string name = GetNewBucketName();

		B2Bucket bucket = await _client.Buckets.Create(name, new B2BucketOptions {
			CacheControl = 600
		});

		// Get bucket to check for info
		List<B2Bucket> bucketList = await _client.Buckets.GetList();

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			await _client.Buckets.Delete(bucket.BucketId);
		}

		B2Bucket? savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.That(savedBucket!.BucketInfo, Is.Not.Null, "Bucket info was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket.BucketInfo.ContainsKey("cache-control"), "Bucket info did not contain Cache-Control");
			Assert.That(savedBucket.BucketInfo["cache-control"], Is.EqualTo("max-age=600"), "Cache-Control values were not equal.");
		});
	}

	[Test]
	public async Task CreateBucketWithLifecycleRulesTest() {
		string name = GetNewBucketName();

		B2Bucket bucket = await _client.Buckets.Create(name, new B2BucketOptions {
			LifecycleRules = new List<B2BucketLifecycleRule> {
				new() {
					DaysFromHidingToDeleting = 30,
					DaysFromUploadingToHiding = null,
					FileNamePrefix = "testing"
				}
			}
		});

		// Get bucket to check for info
		List<B2Bucket> bucketList = await _client.Buckets.GetList();

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			await _client.Buckets.Delete(bucket.BucketId);
		}

		B2Bucket? savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket!.BucketInfo, Is.Not.Null, "Bucket info was null");
			Assert.That(savedBucket.LifecycleRules, Has.Count.EqualTo(1), "Lifecycle rules count was " + savedBucket.LifecycleRules.Count);
		});
		Assert.Multiple(() => {
			Assert.That(savedBucket!.LifecycleRules.First().FileNamePrefix, Is.EqualTo("testing"), "File name prefixes in the first lifecycle rule were not equal.");
			Assert.That(savedBucket.LifecycleRules.First().DaysFromUploadingToHiding, Is.EqualTo(null), "The first lifecycle rule DaysFromUploadingToHiding was not null");
			Assert.That(savedBucket.LifecycleRules.First().DaysFromHidingToDeleting, Is.EqualTo(30), "The first lifecycle rule DaysFromHidingToDeleting was not 30");
		});
	}

	[Test]
	public async Task UpdateBucketWithCacheControlTest() {
		string name = GetNewBucketName();

		B2Bucket bucket = await _client.Buckets.Create(name, new B2BucketOptions { CacheControl = 600 });

		// Update bucket with new info
		bucket = await _client.Buckets.Update(new B2BucketOptions { CacheControl = 300 }, bucket.BucketId);

		// Get bucket to check for info
		List<B2Bucket> bucketList = await _client.Buckets.GetList();

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			await _client.Buckets.Delete(bucket.BucketId);
		}

		B2Bucket? savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.That(savedBucket!.BucketInfo, Is.Not.Null, "Bucket info was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket.BucketInfo.ContainsKey("cache-control"), "Bucket info did not contain Cache-Control");
			Assert.That(savedBucket.BucketInfo["cache-control"], Is.EqualTo("max-age=300"), "Cache-Control values were not equal.");
		});
	}

	[Test]
	public async Task UpdateBucketWithLifecycleRulesTest() {
		string name = GetNewBucketName();

		B2Bucket bucket = await _client.Buckets.Create(name, new B2BucketOptions {
			LifecycleRules = new List<B2BucketLifecycleRule> {
				new() {
					DaysFromHidingToDeleting = 30,
					DaysFromUploadingToHiding = 15,
					FileNamePrefix = "testing"
				}
			}
		});

		// Update bucket with new info
		bucket = await _client.Buckets.Update(new B2BucketOptions {
			LifecycleRules = new List<B2BucketLifecycleRule> {
				new() {
					DaysFromHidingToDeleting = 10,
					DaysFromUploadingToHiding = 10,
					FileNamePrefix = "tested"
				}
			}
		}, bucket.BucketId);

		// Get bucket to check for info
		List<B2Bucket> bucketList = await _client.Buckets.GetList();

		// Clean up
		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			await _client.Buckets.Delete(bucket.BucketId);
		}

		B2Bucket? savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

		Assert.That(savedBucket, Is.Not.Null, "Retrieved bucket was null");
		Assert.Multiple(() => {
			Assert.That(savedBucket!.BucketInfo, Is.Not.Null, "Bucket info was null");
			Assert.That(savedBucket.LifecycleRules, Has.Count.EqualTo(1), "Lifecycle rules count was " + savedBucket.LifecycleRules.Count);
		});
		Assert.That(savedBucket!.LifecycleRules.First().FileNamePrefix, Is.EqualTo("tested"), "File name prefixes in the first lifecycle rule were not equal.");
	}

	[Test]
	public async Task DeleteBucketTest() {
		string name = GetNewBucketName();

		//Creat a bucket to delete
		B2Bucket bucket = await _client.Buckets.Create(name, BucketType.AllPrivate);

		if (!string.IsNullOrEmpty(bucket.BucketId)) {
			B2Bucket deletedBucket = await _client.Buckets.Delete(bucket.BucketId);
			Assert.That(deletedBucket.BucketName, Is.EqualTo(name));
		} else {
			Assert.Fail("The bucket was not deleted. The response did not contain a bucketId.");
		}
	}

	[Test]
	public async Task UpdateBucketTest() {
		string name = GetNewBucketName();

		//Creat a bucket to delete
		B2Bucket bucket = _client.Buckets.Create(name, BucketType.AllPrivate).Result;

		try {
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				B2Bucket updatedBucket = await _client.Buckets.Update(BucketType.AllPublic, bucket.BucketId);
				Assert.That(updatedBucket.BucketType, Is.EqualTo(BucketType.AllPublic.ToString()));
			} else {
				Assert.Fail("The bucket was not deleted. The response did not contain a bucketId.");
			}
		} catch (Exception ex) {
			Assert.Fail(ex.Message);
		} finally {
			await _client.Buckets.Delete(bucket.BucketId);
		}
	}

	[Test]
	public async Task BucketCorsRulesTest() {
		string name = GetNewBucketName();

		B2Bucket bucket = await _client.Buckets.Create(name, new B2BucketOptions {
			CorsRules = new List<B2CorsRule> {
				new() {
					CorsRuleName = "allowAnyHttps",
					AllowedHeaders = new[] { "x-bz-content-sha1", "x-bz-info-*" },
					AllowedOperations = new[] { "b2_upload_file" },
					AllowedOrigins = new[] { "https://*" }
				}
			}
		});

		try {
			List<B2Bucket> list = await _client.Buckets.GetList();
			Assert.That(list, Is.Not.Empty);

			B2Bucket corsBucket = list.First(x => x.BucketId == bucket.BucketId);

			Assert.That(corsBucket.CorsRules.First().CorsRuleName, Is.EqualTo("allowAnyHttps"), "CORS header was not saved or returned for bucket.");
		} finally {
			await _client.Buckets.Delete(bucket.BucketId);
		}
	}

	[Test]
	public async Task BucketCorsRuleUpdateTest() {
		string name = GetNewBucketName();

		B2Bucket bucket = await _client.Buckets.Create(name, new B2BucketOptions {
			CorsRules = new List<B2CorsRule> {
				new() {
					CorsRuleName = "allowAnyHttps",
					AllowedHeaders = new[] { "x-bz-content-sha1", "x-bz-info-*" },
					AllowedOperations = new[] { "b2_upload_file" },
					AllowedOrigins = new[] { "https://*" }
				}
			}
		});

		try {
			_ = await _client.Buckets.Update(new B2BucketOptions {
				CorsRules = new List<B2CorsRule> {
					new() {
						CorsRuleName = "updatedRule",
						AllowedOperations = new[] { "b2_upload_part" },
						AllowedOrigins = new[] { "https://*" }
					}
				}
			}, bucket.Revision, bucket.BucketId);

			List<B2Bucket> list = await _client.Buckets.GetList();
			Assert.That(list, Is.Not.Empty);

			B2Bucket corsBucket = list.First(x => x.BucketId == bucket.BucketId);

			Assert.Multiple(() => {
				Assert.That(corsBucket.CorsRules.First().CorsRuleName, Is.EqualTo("updatedRule"), "CORS header was not updated for bucket.");
				Assert.That(corsBucket.CorsRules.First().AllowedOperations.First(), Is.EqualTo("b2_upload_part"), "CORS header was not updated for bucket.");
			});
		} finally {
			await _client.Buckets.Delete(bucket.BucketId);
		}
	}
}
