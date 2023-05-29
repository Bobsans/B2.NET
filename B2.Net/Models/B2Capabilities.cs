namespace B2.Models;

[Serializable]
public class B2Capabilities {
	public string BucketName { get; }
	public string BucketId { get; }
	public string NamePrefix { get; }
	public string[] Capabilities { get; }

	public B2Capabilities(B2AuthCapabilities authCapabilities) {
		BucketId = authCapabilities.BucketId;
		BucketName = authCapabilities.BucketName;
		Capabilities = authCapabilities.Capabilities;
		NamePrefix = authCapabilities.NamePrefix;
	}
}
