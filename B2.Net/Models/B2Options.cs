namespace B2.Models;

public class B2Options {
	public string AccountId { get; private set; } = null!;
	public string KeyId { get; set; } = null!;
	public string ApplicationKey { get; set; } = null!;
	public string BucketId { get; set; } = null!;

	/// <summary>
	/// Setting this to true will use this bucket by default for all
	/// api calls made from this client. Useful if your app will
	/// only ever use one bucket. Default: false.
	/// </summary>
	public bool PersistBucket { get; set; }

	// State
	public string ApiUrl { get; set; } = null!;
	public string DownloadUrl { get; set; } = null!;
	public string AuthorizationToken { get; set; } = null!;
	public B2Capabilities Capabilities { get; private set; } = null!;

	public long RecommendedPartSize { get; set; }
	public long AbsoluteMinimumPartSize { get; set; }

	/// <summary>
	/// Deprecated: Will always be the same as RecommendedPartSize
	/// </summary>
	public long MinimumPartSize => RecommendedPartSize;

	public int RequestTimeout { get; set; }

	public bool Authenticated { get; private set; }

	public B2Options() {
		PersistBucket = false;
		RequestTimeout = 100;
	}

	public void SetState(B2AuthResponse response) {
		ApiUrl = response.ApiUrl;
		DownloadUrl = response.DownloadUrl;
		AuthorizationToken = response.AuthorizationToken;
		RecommendedPartSize = response.RecommendedPartSize;
		AbsoluteMinimumPartSize = response.AbsoluteMinimumPartSize;
		AccountId = response.AccountId;
		Capabilities = new B2Capabilities(response.Allowed);
		Authenticated = true;
	}
}
