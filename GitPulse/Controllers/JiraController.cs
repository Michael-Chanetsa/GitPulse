using Microsoft.AspNetCore.Mvc;
using GitPulse.Models;
using GitPulse.Services;

namespace GitPulse.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class JiraController : ControllerBase
	{
		private readonly JiraService _jiraService;
		private readonly ILogger<JiraController> _logger;

		public JiraController(JiraService jiraService, ILogger<JiraController> logger)
		{
			_jiraService = jiraService;
			_logger = logger;
		}

		[HttpPost("create-task")]
		public async Task<IActionResult> CreateTask([FromBody] JiraIssueDto dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.Summary))
				{
					return BadRequest("Summary is required");
				}

				var result = await _jiraService.CreateIssueAsync(dto);
				return Ok(new
				{
					message = "Task created successfully in Jira",
					issueKey = result.Key,
					issueId = result.Id,
					issueUrl = result.Self
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create Jira task");
				return StatusCode(500, new { error = "Failed to create Jira task", details = ex.Message });
			}
		}

		[HttpPost("create-from-sonar")]
		public async Task<IActionResult> CreateFromSonarIssue([FromBody] CreateFromSonarRequest request)
		{
			try
			{
				if (request.SonarIssue == null || string.IsNullOrWhiteSpace(request.SonarIssue.Message))
				{
					return BadRequest("Valid issue data is required");
				}

				var dto = _jiraService.CreateIssueFromSonarResult(request.SonarIssue, request.RepositoryName);
				var result = await _jiraService.CreateIssueAsync(dto);

				return Ok(new
				{
					message = "Task created successfully from SonarQube issue",
					issueKey = result.Key,
					issueId = result.Id,
					sonarRule = request.SonarIssue.Rule
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create Jira task from Sonar issue");
				return StatusCode(500, new { error = "Failed to create Jira task", details = ex.Message });
			}
		}

		[HttpPost("create-bulk-from-sonar")]
		public async Task<IActionResult> CreateBulkFromSonar([FromBody] CreateBulkFromSonarRequest request)
		{
			try
			{
				var results = new List<object>();
				var errors = new List<string>();

				foreach (var issue in request.SonarIssues)
				{
					try
					{
						var dto = _jiraService.CreateIssueFromSonarResult(issue, request.RepositoryName);
						var result = await _jiraService.CreateIssueAsync(dto);
						results.Add(new { issueKey = result.Key, sonarRule = issue.Rule, success = true });
					}
					catch (Exception ex)
					{
						errors.Add($"Failed to create issue for rule {issue.Rule}: {ex.Message}");
						results.Add(new { sonarRule = issue.Rule, success = false, error = ex.Message });
					}
				}

				return Ok(new
				{
					message = $"Processed {request.SonarIssues.Count} issues",
					successful = results.Count(r => (bool)r.GetType().GetProperty("success")?.GetValue(r, null)!),
					failed = errors.Count,
					results,
					errors
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to process bulk Jira task creation");
				return StatusCode(500, new { error = "Failed to process bulk creation", details = ex.Message });
			}
		}

		[HttpGet("test-connection")]
		public async Task<IActionResult> TestConnection()
		{
			try
			{
				var isConnected = await _jiraService.TestConnectionAsync();
				return Ok(new { connected = isConnected, message = isConnected ? "Connection successful" : "Connection failed" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error testing Jira connection");
				return StatusCode(500, new { error = "Connection test failed", details = ex.Message });
			}
		}
	}

	public class CreateFromSonarRequest
	{
		public SonarIssueDto SonarIssue { get; set; } = new();
		public string RepositoryName { get; set; } = string.Empty;
	}

	public class CreateBulkFromSonarRequest
	{
		public List<SonarIssueDto> SonarIssues { get; set; } = new();
		public string RepositoryName { get; set; } = string.Empty;
	}
}
