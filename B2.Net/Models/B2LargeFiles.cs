namespace B2.Models;

[Serializable]
public class B2UploadPartUrl {
	public string FileId { get; set; } = null!;
	public string UploadUrl { get; set; } = null!;
	public string AuthorizationToken { get; set; } = null!;
}

[Serializable]
public class B2UploadPart {
	public string FileId { get; set; } = null!;
	public int PartNumber { get; set; }
	public int Length => ContentLength;
	public string Sha1 => ContentSha1;
	public int ContentLength { get; set; }
	public string ContentSha1 { get; set; } = null!;
}

[Serializable]
public class B2LargeFileParts {
	public int? NextPartNumber { get; set; }
	public List<B2LargeFilePart> Parts { get; set; } = null!;
}

[Serializable]
public class B2LargeFilePart {
	public string FileId { get; set; } = null!;
	public int PartNumber { get; set; }
	public int ContentLength { get; set; }
	public string ContentSha1 { get; set; } = null!;
	public double UploadTimestamp { get; set; }
}

[Serializable]
public class B2CancelledFile {
	public string FileId { get; set; } = null!;
	public string AccountId { get; set; } = null!;
	public string BucketId { get; set; } = null!;
	public string FileName { get; set; } = null!;
}

[Serializable]
public class B2IncompleteLargeFiles {
	public string NextFileId { get; set; } = null!;
	public List<B2File> Files { get; set; } = null!;
}
