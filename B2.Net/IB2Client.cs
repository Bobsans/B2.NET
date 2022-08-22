using B2.Models;

namespace B2; 

public interface IB2Client {
	IBuckets Buckets { get; }
	IFiles Files { get; }
	ILargeFiles LargeFiles { get; }
	B2Capabilities Capabilities { get; }

	Task<B2Options> Authorize(CancellationToken cancelToken = default);
}
