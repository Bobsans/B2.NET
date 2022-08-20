using System.Net.Http;
using B2.Models;
using Newtonsoft.Json;

namespace B2.Http.RequestGenerators;

public static class FileMetaDataRequestGenerators {
	static class Endpoints {
		public const string LIST = "b2_list_file_names";
		public const string VERSIONS = "b2_list_file_versions";
		public const string HIDE = "b2_hide_file";
		public const string INFO = "b2_get_file_info";
	}

	public static HttpRequestMessage GetList(B2Options options, string bucketId, string startFileName = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
		string body = "{\"bucketId\":\"" + bucketId + "\"";

		if (!string.IsNullOrEmpty(startFileName)) {
			body += ", \"startFileName\":" + JsonConvert.ToString(startFileName);
		}

		if (maxFileCount.HasValue) {
			body += ", \"maxFileCount\":" + maxFileCount.Value;
		}

		if (!string.IsNullOrEmpty(prefix)) {
			body += ", \"prefix\":" + JsonConvert.ToString(prefix);
		}

		if (!string.IsNullOrEmpty(delimiter)) {
			body += ", \"delimiter\":" + JsonConvert.ToString(delimiter);
		}

		body += "}";
		return BaseRequestGenerator.PostRequest(Endpoints.LIST, body, options);
	}

	public static HttpRequestMessage ListVersions(B2Options options, string bucketId, string startFileName = "", string startFileId = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
		string body = "{\"bucketId\":\"" + bucketId + "\"";

		if (!string.IsNullOrEmpty(startFileName)) {
			body += ", \"startFileName\":" + JsonConvert.ToString(startFileName);
		}

		if (!string.IsNullOrEmpty(startFileId)) {
			body += ", \"startFileId\":\"" + startFileId + "\"";
		}

		if (maxFileCount.HasValue) {
			body += ", \"maxFileCount\":" + maxFileCount.Value;
		}

		if (!string.IsNullOrEmpty(prefix)) {
			body += ", \"prefix\":" + JsonConvert.ToString(prefix);
		}

		if (!string.IsNullOrEmpty(delimiter)) {
			body += ", \"delimiter\":" + JsonConvert.ToString(delimiter);
		}

		body += "}";
		
		return BaseRequestGenerator.PostRequest(Endpoints.VERSIONS, body, options);
	}

	public static HttpRequestMessage HideFile(B2Options options, string bucketId, string fileName = "", string fileId = "") {
		string body = "{\"bucketId\":\"" + bucketId + "\"";
		
		if (!string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(fileId)) {
			body += ", \"fileName\":" + JsonConvert.ToString(fileName);
		}

		if (!string.IsNullOrEmpty(fileId)) {
			body += ", \"fileId\":\"" + fileId + "\"";
		}

		body += "}";
		
		return BaseRequestGenerator.PostRequest(Endpoints.HIDE, body, options);
	}

	public static HttpRequestMessage GetInfo(B2Options options, string fileId) {
		string json = JsonConvert.SerializeObject(new { fileId });
		return BaseRequestGenerator.PostRequest(Endpoints.INFO, json, options);
	}
}
