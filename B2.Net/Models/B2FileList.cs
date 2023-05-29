namespace B2.Models;

[Serializable]
public class B2FileList {
	public string NextFileName { get; set; } = null!;
	public string NextFileId { get; set; } = null!;
	public List<B2File> Files { get; set; } = null!;
}
