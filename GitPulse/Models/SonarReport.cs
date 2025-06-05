namespace GitPulse.Models
{
	public class SonarReport
	{
		public string ProjectKey { get; set; } = string.Empty;
		public int Bugs { get; set; }
		public int Vulnerabilities { get; set; }
		public int CodeSmells { get; set; }
		public double? Coverage { get; set; }
		public double? DuplicatedLinesDensity { get; set; }
		public List<IssueDto> TopIssues { get; set; } = new();
		public string QualityGate { get; set; } = "N/A";
	}

	public class SummaryResult
	{
		public string Text { get; set; } = string.Empty;
		public string QualityGate { get; set; } = "N/A";
		public List<IssueDto> TopIssues { get; set; } = new();
	}



	public class IssueDto
	{
		public string Rule { get; set; }
		public string Message { get; set; }
		public string Severity { get; set; }
	}
}
