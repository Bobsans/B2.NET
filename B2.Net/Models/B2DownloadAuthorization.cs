﻿namespace B2.Models;

public class B2DownloadAuthorization {
	public string BucketId { get; set; } = null!;
	public string FileNamePrefix { get; set; } = null!;
	public string AuthorizationToken { get; set; } = null!;
}
