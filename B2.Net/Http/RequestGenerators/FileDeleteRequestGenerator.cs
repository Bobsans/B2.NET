using System.Net.Http;
using B2.Models;
using Newtonsoft.Json;

namespace B2.Http.RequestGenerators;

public static class FileDeleteRequestGenerator {
	static class Endpoints {
		public const string DELETE = "b2_delete_file_version";
	}

	public static HttpRequestMessage Delete(B2Options options, string fileId, string fileName) {
		return BaseRequestGenerator.PostRequest(
			Endpoints.DELETE,
			JsonConvert.SerializeObject(new { fileId, fileName }),
			options
		);
	}
}
