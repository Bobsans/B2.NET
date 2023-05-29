namespace B2.Models;

[Serializable]
public class B2File {
	public string FileId { get; set; } = null!;
	public string FileName { get; set; } = null!;
	public string Action { get; set; } = null!;
	public long Size { get; set; }
	public double UploadTimestamp { get; set; }

	public byte[] FileData { get; set; } = null!;

	// Uploaded File Response
	public int ContentLength { get; set; }
	public string ContentSha1 { get; set; } = null!;
	public string ContentType { get; set; } = null!;

	public Dictionary<string, string> FileInfo { get; set; } = null!;
	// End
}
