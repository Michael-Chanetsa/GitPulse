using System.Text.Json.Serialization;

namespace GitPulse.Models
{
	public class JiraIssueDto
	{
		public string Summary { get; set; } = string.Empty;
		public object Description { get; set; } = string.Empty;
		public string IssueType { get; set; } = "Task";
		public string? Priority { get; set; }
		public string? Component { get; set; }
		public List<string>? Labels { get; set; }
	}

	public class JiraCreateResponse
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = string.Empty;

		[JsonPropertyName("key")]
		public string Key { get; set; } = string.Empty;

		[JsonPropertyName("self")]
		public string Self { get; set; } = string.Empty;
	}

	public class SonarIssueDto
	{
		public string Rule { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public string Severity { get; set; } = string.Empty;
		public string? Component { get; set; }
		public int? Line { get; set; }
		public string? Type { get; set; }
		public string? FilePath { get; set; }
	}

	public class JiraConfiguration
	{
		public string BaseUrl { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string ApiToken { get; set; } = string.Empty;
		public string ProjectKey { get; set; } = string.Empty;
	}
}