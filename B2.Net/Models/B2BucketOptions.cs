using System.Collections.Generic;

namespace B2.Models;

public class B2BucketOptions {
	public BucketType BucketType { get; set; } = BucketType.allPrivate;
	public int CacheControl { get; set; }
	public List<B2BucketLifecycleRule> LifecycleRules { get; set; } = null!;
	public List<B2CorsRule> CORSRules { get; set; } = null!;
}
