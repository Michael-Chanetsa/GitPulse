using Microsoft.AspNetCore.Mvc;
using GitPulse.Models;
using GitPulse.Services;

namespace GitPulse.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class JiraController : ControllerBase
	{
		private readonly JiraService _jira;

		public JiraController(JiraService jira)
		{
			_jira = jira;
		}

		[HttpPost("create-task")]
		public async Task<IActionResult> CreateTask([FromBody] JiraIssueDto dto)
		{
			try
			{
				var result = await _jira.CreateIssueAsync(dto);
				return Ok(new { message = "Task created in Jira", result });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Failed to create Jira task: {ex.Message}");
			}
		}

		[HttpPost("create-task-from-issue")]
		public async Task<IActionResult> CreateFromSonarIssue([FromBody] IssueDto issue)
		{
			try
			{
				var dto = new JiraIssueDto
				{
					Summary = $"[{issue.Severity}] {issue.Message}",
					Description = new
					{
						type = "doc",
						version = 1,
						content = new[]
					{
						new
						{
							type = "paragraph",
							content = new[]
							{
								new { type = "text", text = $"Auto-created from GitPulse based on Sonar rule: {issue.Rule}" }
							}
						}
					}
								},
								IssueType = new IssueType { Name = "Task" }
							};


				var result = await _jira.CreateIssueAsync(dto);
				return Ok(new { message = "Task created in Jira", result });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Failed to create Jira task: {ex.Message}");
			}
		}

	}
}
