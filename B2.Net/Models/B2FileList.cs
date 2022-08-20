using System.Collections.Generic;

namespace B2.Models; 

public class B2FileList {
	public string NextFileName { get; set; }
	public string NextFileId { get; set; }
	public List<B2File> Files { get; set; }
}
