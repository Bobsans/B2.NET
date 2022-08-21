using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using B2.Models;

namespace B2;

public static class Utilities {
	/// <summary>
	/// Create the B2 Authorization header. Base64 encoded accountId:applicationKey.
	/// </summary>
	public static string CreateAuthorizationHeader(string accountId, string applicationKey) {
		return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(accountId + ":" + applicationKey))}";
	}

	public static T? DeserializeModel<T>(string json) where T : new() {
		return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
	}

	public static async Task CheckForErrors(HttpResponseMessage response, string? callingApi = null) {
		if (!response.IsSuccessStatusCode) {
			// Should retry
			bool retry = response.StatusCode is (HttpStatusCode)429 or HttpStatusCode.RequestTimeout or HttpStatusCode.ServiceUnavailable;
			string content = await response.Content.ReadAsStringAsync();

			B2Error? b2Error;

			try {
				b2Error = DeserializeModel<B2Error>(content);
			} catch (Exception ex) {
				throw new Exception("Serialization of the response failed. See inner exception for response contents and serialization error.", ex);
			}

			if (b2Error != null) {
				// If calling API is supplied, append to the error message
				if (callingApi != null && b2Error.Status == 401) {
					b2Error.Message = $"Unauthorized error when operating on {callingApi}. Are you sure the key you are using has access? {b2Error.Message}";
				}

				throw new B2Exception(b2Error.Message) {
					Status = b2Error.Status,
					Code = b2Error.Code,
					ShouldRetryRequest = retry
				};
			}
		}
	}

	public static string GetSha1Hash(byte[] fileData) {
		using SHA1 sha1 = SHA1.Create();
		return BitConverter.ToString(sha1.ComputeHash(fileData)).Replace("-", "").ToLowerInvariant();
	}

	public static string DetermineBucketId(B2Options options, string bucketId) {
		// Check for a persistant bucket
		if (!options.PersistBucket && string.IsNullOrEmpty(bucketId)) {
			throw new ArgumentNullException(nameof(bucketId), "You must either Persist a Bucket or provide a BucketId in the method call.");
		}

		// Are we persisting buckets? If so use the one from settings
		return options.PersistBucket ? options.BucketId : bucketId;
	}

	class B2Error {
		public int Status { get; set; }
		public string Code { get; set; } = null!;
		public string Message { get; set; } = null!;
	}
}

public static class B2StringExtension {
	public static string B2UrlEncode(this string str) {
		return str == "/" ? str : Uri.EscapeDataString(str).Replace("%2F", "/");
	}

	public static string B2UrlDecode(this string str) {
		return str == "+" ? " " : Uri.UnescapeDataString(str);
	}
}
