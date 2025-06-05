using System.Text;
using GitPulse.Models;

namespace GitPulse.Services
{
	public class SummaryService
	{
		public SummaryResult GenerateSummary(SonarReport report)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"🧠 **Code Quality Summary** for project `{report.ProjectKey}`:\n");
			sb.AppendLine($"- **Bugs**: {report.Bugs}");
			sb.AppendLine($"- **Vulnerabilities**: {report.Vulnerabilities}");
			sb.AppendLine($"- **Code Smells**: {report.CodeSmells}");
			sb.AppendLine($"- **Coverage**: {report.Coverage}%");
			sb.AppendLine($"- **Duplicated Lines**: {report.DuplicatedLinesDensity}%\n");

			if (report.Bugs > 0 || report.Vulnerabilities > 0)
				sb.AppendLine("🚨 **Action Required:** Address security vulnerabilities and logic bugs ASAP.");
			else
				sb.AppendLine("✅ No critical bugs or vulnerabilities detected.");

			if (report.CodeSmells > 100)
				sb.AppendLine("⚠️ High number of code smells — consider refactoring for maintainability.");

			if (report.Coverage < 30)
				sb.AppendLine("🧪 Code coverage is very low — recommend writing unit tests.");

			sb.AppendLine("\n🔍 **Top Issues:**");
			foreach (var issue in report.TopIssues.Take(5))
			{
				sb.AppendLine($"- [{issue.Severity}] {issue.Message} (Rule: {issue.Rule})");
			}

			return new SummaryResult
			{
				Text = sb.ToString(),
				QualityGate = report.QualityGate ?? "N/A",
				TopIssues = report.TopIssues
			};
		}
	}
}
