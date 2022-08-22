using System.Text.Json.Serialization;
using B2.Models;

namespace B2.Http.RequestGenerators;

public static class FileCopyRequestGenerators {
	static class Endpoints {
		public const string COPY = "b2_copy_file";
	}

	public static HttpRequestMessage Copy(B2Options options, string sourceFileId, string fileName, B2MetadataDirective metadataDirective, string? contentType = null, Dictionary<string, string>? fileInfo = null, string? range = null, string? destinationBucketId = null) {
		B2CopyFileRequestPayload payload = new() {
			SourceFileId = sourceFileId,
			FileName = fileName,
			MetadataDirective = metadataDirective,
			Range = range,
			DestinationBucketId = destinationBucketId
		};

		if (metadataDirective == B2MetadataDirective.REPLACE) {
			payload.ContentType = string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType;
		}

		// File Info
		if (metadataDirective == B2MetadataDirective.REPLACE && fileInfo is { Count: > 0 }) {
			payload.FileInfo = fileInfo;
		}

		return BaseRequestGenerator.PostRequestJson(Endpoints.COPY, payload, options);
	}
}

class B2CopyFileRequestPayload {
	public string SourceFileId { get; set; } = null!;
	public string? DestinationBucketId { get; set; }
	public string FileName { get; set; } = null!;
	public string? Range { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public B2MetadataDirective? MetadataDirective { get; set; }

	public string? ContentType { get; set; }

	public Dictionary<string, string>? FileInfo { get; set; }
	// public string? FileRetention { get; set; }
	// public string? LegalHold { get; set; }
	// public string? sourceServerSideEncryption { get; set; }
	// public string? destinationServerSideEncryption { get; set; }
}
