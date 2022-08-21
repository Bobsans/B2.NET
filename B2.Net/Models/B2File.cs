using System;
using System.Collections.Generic;

namespace B2.Models;

public class B2File {
	public string FileId { get; set; } = null!;
	public string FileName { get; set; } = null!;
	public string Action { get; set; } = null!;
	public long Size { get; set; }
	public string UploadTimestamp { get; set; } = null!;

	public byte[] FileData { get; set; } = null!;

	// Uploaded File Response
	public string ContentLength { get; set; } = null!;
	public string ContentSHA1 { get; set; } = null!;
	public string ContentType { get; set; } = null!;

	public Dictionary<string, string> FileInfo { get; set; } = null!;
	// End

	public DateTime UploadTimestampDate {
		get {
			if (!string.IsNullOrEmpty(UploadTimestamp)) {
				DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				return epoch.AddMilliseconds(double.Parse(UploadTimestamp));
			}

			return DateTime.Now;
		}
	}
}
