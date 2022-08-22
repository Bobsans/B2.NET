using B2.Models;

namespace B2.Http.RequestGenerators;

public static class FileDeleteRequestGenerator {
	static class Endpoints {
		public const string DELETE = "b2_delete_file_version";
	}

	public static HttpRequestMessage Delete(B2Options options, string fileId, string fileName) {
		return BaseRequestGenerator.PostRequestJson(Endpoints.DELETE, new {
			fileId,
			fileName
		}, options);
	}
}
