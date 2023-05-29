using System.Net;
using B2.Http;
using B2.Http.RequestGenerators;
using B2.Models;

namespace B2;

public class B2Client {
	readonly B2Options _options;

	public Buckets Buckets { get; private set; }
	public Files Files { get; private set; }
	public LargeFiles LargeFiles { get; private set; }

	public B2Capabilities Capabilities {
		get {
			if (_options.Authenticated) {
				return _options.Capabilities;
			}

			throw new NotAuthorizedException(
				"You attempted to load the capabilities of this key before authenticating with Backblaze. " +
				"You must Authorize before you can access Capabilities."
			);
		}
	}

	public B2Client(B2Options options) {
		_options = options;

		if (options.AutomaticAuth) {
			Authorize();
		}

		Buckets = new Buckets(options);
		Files = new Files(options);
		LargeFiles = new LargeFiles(options);
	}

	public B2Client(string keyId, string applicationKey, int requestTimeout = 100) : this(new B2Options {
		KeyId = keyId,
		ApplicationKey = applicationKey,
		RequestTimeout = requestTimeout
	}) {}

	public B2Client Authorize() => AuthorizeAsync().Result;

	public async Task<B2Client> AuthorizeAsync(CancellationToken cancellationToken = default) {
		if (_options.Authenticated) {
			return this;
		}

		if (_options.KeyId == null || _options.ApplicationKey == null) {
			throw new AuthorizationException("Either KeyId or ApplicationKey were not specified.");
		}

		HttpClient client = HttpClientFactory.CreateHttpClient(_options.RequestTimeout);
		HttpResponseMessage response = await client.SendAsync(AuthRequestGenerator.Authorize(_options), cancellationToken);

		string json = await response.Content.ReadAsStringAsync(cancellationToken);

		if (response.IsSuccessStatusCode) {
			_options.SetState(Utils.Deserialize<B2AuthResponse>(json)!);
		} else if (response.StatusCode == HttpStatusCode.Unauthorized) {
			throw new AuthorizationException(
				"If you are using an Application key and not a Master key, make sure that you are " +
				"supplying the Key ID and Key Value for that Application Key. " +
				"Do not mix your Account ID with your Application Key."
			);
		} else {
			throw new AuthorizationException(json);
		}

		return this;
	}
}
