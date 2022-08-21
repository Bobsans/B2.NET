using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;

namespace B2;

public class B2Client : IB2Client {
	B2Options options;
	B2Capabilities capabilities = null!;

	public B2Capabilities Capabilities {
		get {
			if (options.Authenticated) {
				return capabilities;
			}

			throw new NotAuthorizedException("You attempted to load the capabilities of this key before authenticating with Backblaze. You must Authorize before you can access Capabilities.");
		}
	}

	/// <summary>
	/// If you specify authorizeOnInitialize = false, you MUST call Initialize() once before you use the client.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="authorizeOnInitialize"></param>
	public B2Client(B2Options options, bool authorizeOnInitialize = true) {
		// Should we authorize on the class initialization?
		if (authorizeOnInitialize) {
			this.options = Authorize(options);
			Buckets = new Buckets(options);
			Files = new Files(options);
			LargeFiles = new LargeFiles(options);
			capabilities = options.Capabilities;
		} else {
			// If not, then the user will have to Initialize() before making any calls.
			this.options = options;
		}
	}

	/// <summary>
	/// Simple method for instantiating the B2Client. Does auth for you. See https://www.backblaze.com/b2/docs/application_keys.html for details on application keys.
	/// This method defaults to not persisting a bucket. Manually build the options object if you wish to do that.
	/// </summary>
	/// <param name="keyId"></param>
	/// <param name="applicationKey"></param>
	/// <param name="requestTimeout"></param>
	public B2Client(string keyId, string applicationKey, int requestTimeout = 100) {
		options = new B2Options {
			KeyId = keyId,
			ApplicationKey = applicationKey,
			RequestTimeout = requestTimeout
		};
		options = Authorize(options);

		Buckets = new Buckets(options);
		Files = new Files(options);
		LargeFiles = new LargeFiles(options);
		capabilities = options.Capabilities;
	}

	/// <summary>
	/// Simple method for instantiating the B2Client. Does auth for you. See https://www.backblaze.com/b2/docs/application_keys.html for details on application keys.
	/// This method defaults to not persisting a bucket. Manually build the options object if you wish to do that.
	/// </summary>
	/// <param name="accountId"></param>
	/// <param name="applicationKey"></param>
	/// <param name="keyId"></param>
	/// <param name="requestTimeout"></param>
	[Obsolete("Use B2Client(string keyId, string applicationKey, int requestTimeout = 100) instead as AccountId is no longer needed")]
	public B2Client(string accountId, string applicationKey, string keyId, int requestTimeout = 100) : this(keyId, applicationKey, requestTimeout) {}

	public IBuckets Buckets { get; private set; } = null!;
	public IFiles Files { get; private set; } = null!;
	public ILargeFiles LargeFiles { get; private set; } = null!;

	/// <summary>
	/// Only call this method if you created a B2Client with authorizeOnInitialize = false. This method of using B2.NET is considered in Beta, as it has not been extensively tested.
	/// </summary>
	/// <returns></returns>
	public void Initialize() {
		options = Authorize(options);
		Buckets = new Buckets(options);
		Files = new Files(options);
		LargeFiles = new LargeFiles(options);
		capabilities = options.Capabilities;
	}

	/// <summary>
	/// Authorize against the B2 storage service. Requires that KeyId and ApplicationKey on the options object be set.
	/// </summary>
	/// <returns>B2Options containing the download url, new api url, AccountID and authorization token.</returns>
	public async Task<B2Options> Authorize(CancellationToken cancelToken = default) {
		return await AuthorizeAsync(options);
	}

	public static async Task<B2Options> AuthorizeAsync(string keyId, string applicationKey) {
		return await AuthorizeAsync(new B2Options { ApplicationKey = applicationKey, KeyId = keyId });
	}

	public static B2Options Authorize(string keyId, string applicationKey) {
		return Authorize(new B2Options { ApplicationKey = applicationKey, KeyId = keyId });
	}

	/// <summary>
	/// Requires that KeyId and ApplicationKey on the options object be set. If you are using an application key you must specify the accountId, the keyId, and the applicationKey.
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public static async Task<B2Options> AuthorizeAsync(B2Options options) {
		// Return if already authenticated.
		if (options.Authenticated) {
			return options;
		}

		if (options.KeyId == null || options.ApplicationKey == null) {
			throw new AuthorizationException("Either KeyId or ApplicationKey were not specified.");
		}

		HttpClient client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
		HttpResponseMessage response = await client.SendAsync(AuthRequestGenerator.Authorize(options));

		string jsonResponse = await response.Content.ReadAsStringAsync();

		if (response.IsSuccessStatusCode) {
			options.SetState(Utilities.DeserializeModel<B2AuthResponse>(jsonResponse)!);
		} else if (response.StatusCode == HttpStatusCode.Unauthorized) {
			// Return a better exception because of confusing Keys api.
			throw new AuthorizationException("If you are using an Application key and not a Master key, make sure that you are supplying the Key ID and Key Value for that Application Key. Do not mix your Account ID with your Application Key.");
		} else {
			throw new AuthorizationException(jsonResponse);
		}

		return options;
	}

	/// <summary>
	/// Requires that KeyId and ApplicationKey on the options object be set. If you are using an application key you must specify the accountId, the keyId, and the applicationKey.
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public static B2Options Authorize(B2Options options) => AuthorizeAsync(options).Result;
}
