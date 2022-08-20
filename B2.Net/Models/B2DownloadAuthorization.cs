namespace B2.Models; 

public class B2DownloadAuthorization {
	public string BucketId { get; set; }
	public string FileNamePrefix { get; set; }
	public string AuthorizationToken { get; set; }
}
