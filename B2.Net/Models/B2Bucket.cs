namespace B2.Models;

public class B2Bucket {
	public string BucketId { get; set; } = null!;
	public string BucketName { get; set; } = null!;
	public string BucketType { get; set; } = null!;
	public Dictionary<string, string> BucketInfo { get; set; } = null!;
	public List<B2BucketLifecycleRule> LifecycleRules { get; set; } = null!;
	public List<B2CorsRule> CorsRules { get; set; } = null!;
	public int Revision { get; set; }
}

public class B2BucketLifecycleRule {
	public int? DaysFromHidingToDeleting { get; set; }
	public int? DaysFromUploadingToHiding { get; set; }
	public string FileNamePrefix { get; set; } = null!;
}

public class B2BucketListDeserializeModel {
	public List<B2Bucket> Buckets { get; set; } = null!;
}
