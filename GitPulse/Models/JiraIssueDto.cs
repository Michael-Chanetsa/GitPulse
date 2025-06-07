namespace GitPulse.Models
{
	public class JiraIssueDto
	{
		public string Summary { get; set; } = string.Empty;
		public object Description { get; set; } = string.Empty;
		public IssueType IssueType { get; set; } 
	}

	public class IssueType
	{
		public string Name { get; set; } = "Task";
	}
}
