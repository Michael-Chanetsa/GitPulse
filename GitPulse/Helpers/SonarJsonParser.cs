using System.Text.Json;
using GitPulse.Models;

namespace GitPulse.Helpers
{
	public static class SonarJsonParser
	{
		public static SonarReport ParseReport(string metricsJson, string issuesJson)
		{
			var report = new SonarReport();

			// 🔍 Parse Metrics JSON
			using (JsonDocument doc = JsonDocument.Parse(metricsJson))
			{
				var root = doc.RootElement;

				report.ProjectKey = root.GetProperty("component").GetProperty("key").GetString();

				if (root.GetProperty("component").TryGetProperty("measures", out var measures))
				{
					foreach (var measure in measures.EnumerateArray())
					{
						string metric = measure.GetProperty("metric").GetString();
						string value = measure.GetProperty("value").GetString();

						switch (metric)
						{
							case "bugs":
								report.Bugs = int.Parse(value);
								break;
							case "vulnerabilities":
								report.Vulnerabilities = int.Parse(value);
								break;
							case "code_smells":
								report.CodeSmells = int.Parse(value);
								break;
							case "coverage":
								report.Coverage = double.TryParse(value, out double coverage) ? coverage : 0.0;
								break;
							case "duplicated_lines_density":
								report.DuplicatedLinesDensity = double.TryParse(value, out double dup) ? dup : 0.0;
								break;
						}
					}
				}
			}

			// 🔍 Parse Top Issues JSON
			var issues = new List<IssueDto>();

			using (JsonDocument doc = JsonDocument.Parse(issuesJson))
			{
				var root = doc.RootElement;

				if (root.TryGetProperty("issues", out var issueArray))
				{
					foreach (var item in issueArray.EnumerateArray())
					{
						issues.Add(new IssueDto
						{
							Rule = item.GetProperty("rule").GetString(),
							Severity = item.GetProperty("severity").GetString(),
							Message = item.GetProperty("message").GetString()
						});
					}
				}
			}

			report.TopIssues = issues;
			return report;
		}
	}
}
