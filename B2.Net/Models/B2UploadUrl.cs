namespace B2.Models;

[Serializable]
public class B2UploadUrl {
	public string BucketId { get; set; } = null!;
	public string UploadUrl { get; set; } = null!;
	public string AuthorizationToken { get; set; } = null!;
}
