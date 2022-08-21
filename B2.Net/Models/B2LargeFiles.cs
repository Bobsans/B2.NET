using System.Collections.Generic;

namespace B2.Models;

public class B2UploadPartUrl {
	public string FileId { get; set; } = null!;
	public string UploadUrl { get; set; } = null!;
	public string AuthorizationToken { get; set; } = null!;
}

public class B2UploadPart {
	public string FileId { get; set; } = null!;
	public int PartNumber { get; set; }
	public int Length => ContentLength;
	public string SHA1 => ContentSHA1;
	public int ContentLength { get; set; }
	public string ContentSHA1 { get; set; } = null!;
}

public class B2LargeFileParts {
	public int NextPartNumber { get; set; }
	public List<B2LargeFilePart> Parts { get; set; } = null!;
}

public class B2LargeFilePart {
	public string FileId { get; set; } = null!;
	public int PartNumber { get; set; }
	public string ContentLength { get; set; } = null!;
	public string ContentSha1 { get; set; } = null!;
	public string UploadTimestamp { get; set; } = null!;
}

public class B2CancelledFile {
	public string FileId { get; set; } = null!;
	public string AccountId { get; set; } = null!;
	public string BucketId { get; set; } = null!;
	public string FileName { get; set; } = null!;
}

public class B2IncompleteLargeFiles {
	public string NextFileId { get; set; } = null!;
	public List<B2File> Files { get; set; } = null!;
}
