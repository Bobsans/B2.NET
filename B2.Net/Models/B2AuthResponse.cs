namespace B2.Models;

public class B2AuthResponse {
	public string AccountId { get; set; } = null!;
	public string ApiUrl { get; set; } = null!;
	public string AuthorizationToken { get; set; } = null!;
	public string DownloadUrl { get; set; } = null!;
	public string S3ApiUrl { get; set; } = null!;
	public long RecommendedPartSize { get; set; }
	public long AbsoluteMinimumPartSize { get; set; }

	[Obsolete("Use RecommendedPartSize instead")]
	public long MinimumPartSize { get; set; }

	public B2AuthCapabilities Allowed { get; set; } = null!;
}

public class B2AuthCapabilities {
	public string BucketId { get; set; } = null!;
	public string BucketName { get; set; } = null!;
	public string NamePrefix { get; set; } = null!;
	public string[] Capabilities { get; set; } = null!;
}
