using B2.Models;

namespace B2.Http.RequestGenerators;

public static class FileMetaDataRequestGenerators {
	static class Endpoints {
		public const string LIST = "b2_list_file_names";
		public const string VERSIONS = "b2_list_file_versions";
		public const string HIDE = "b2_hide_file";
		public const string INFO = "b2_get_file_info";
	}

	public static HttpRequestMessage GetList(B2Options options, string bucketId, string? startFileName = null, int? maxFileCount = null, string? prefix = null, string? delimiter = null) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.LIST, new {
			bucketId, startFileName, maxFileCount, prefix, delimiter
		}, options);
	}

	public static HttpRequestMessage ListVersions(B2Options options, string bucketId, string? startFileName = null, string? startFileId = null, int? maxFileCount = null, string? prefix = null, string? delimiter = null) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.VERSIONS, new {
			bucketId, startFileName, startFileId, maxFileCount, prefix, delimiter
		}, options);
	}

	public static HttpRequestMessage HideFile(B2Options options, string bucketId, string? fileName = null, string? fileId = null) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.HIDE, new {
			bucketId, fileName, fileId
		}, options);
	}

	public static HttpRequestMessage GetInfo(B2Options options, string fileId) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.INFO, new {
			fileId
		}, options);
	}
}
