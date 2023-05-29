namespace B2.Models;

[Serializable]
public class B2BucketOptions {
	public BucketType BucketType { get; set; } = BucketType.AllPrivate;
	public int CacheControl { get; set; }
	public List<B2BucketLifecycleRule> LifecycleRules { get; set; } = null!;
	public List<B2CorsRule> CorsRules { get; set; } = null!;
}
