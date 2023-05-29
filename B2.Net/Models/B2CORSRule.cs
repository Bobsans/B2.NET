namespace B2.Models;

[Serializable]
public class B2CorsRule {
	public string CorsRuleName { get; set; } = null!;
	public string[] AllowedOrigins { get; set; } = null!;
	public string[] AllowedOperations { get; set; } = null!;
	public string[] AllowedHeaders { get; set; } = null!;
	public string[] ExposeHeaders { get; set; } = null!;
	public int MaxAgeSeconds { get; set; }
}
