using GitPulse.Services;
using Microsoft.AspNetCore.Mvc;
using GitPulse.Models;
using GitPulse.Helpers;


namespace GitPulse.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SonarController : ControllerBase
	{
		private readonly SonarService _sonarService;

		public SonarController(SonarService sonarService)
		{
			_sonarService = sonarService;
		}

		/// <summary>
		/// Gets key SonarQube metrics for the specified project.
		/// </summary>
		/// <param name="projectKey">The project key used in SonarQube</param>
		[HttpGet("metrics/{projectKey}")]
		public async Task<IActionResult> GetMetrics(string projectKey)
		{
			try
			{
				var metricsJson = await _sonarService.GetMetricsAsync(projectKey);
				return Content(metricsJson, "application/json");
			}
			catch (HttpRequestException ex)
			{
				return StatusCode(500, $"Failed to retrieve metrics: {ex.Message}");
			}
		}

		/// <summary>
		/// Gets the Quality Gate status for the specified project.
		/// </summary>
		/// <param name="projectKey">The project key used in SonarQube</param>
		[HttpGet("quality-gate/{projectKey}")]
		public async Task<IActionResult> GetQualityGate(string projectKey)
		{
			try
			{
				var qualityGateJson = await _sonarService.GetQualityGateStatusAsync(projectKey);
				return Content(qualityGateJson, "application/json");
			}
			catch (HttpRequestException ex)
			{
				return StatusCode(500, $"Failed to retrieve quality gate: {ex.Message}");
			}
		}

		[HttpGet("summary/{projectKey}")]
		public async Task<IActionResult> GetSummary(
	string projectKey,
	[FromServices] SonarService sonarService,
	[FromServices] SummaryService summaryService)
		{
			try
			{
				var metricsJson = await sonarService.GetMetricsAsync(projectKey);
				var issuesJson = await sonarService.GetTopIssuesAsync(projectKey);

				var report = SonarJsonParser.ParseReport(metricsJson, issuesJson);
				var summaryResult = summaryService.GenerateSummary(report);

				// Return summaryResult.Text (a string), not the entire object
				return Ok(new
				{
					projectKey,
					summary = summaryResult.Text,
					qualityGate = summaryResult.QualityGate,
					topIssues = report.TopIssues,
					bugs = report.Bugs,
					vulnerabilities = report.Vulnerabilities,
					codeSmells = report.CodeSmells,
					coverage = report.Coverage,
					duplicatedLinesDensity = report.DuplicatedLinesDensity
				});
			}
			catch (HttpRequestException ex)
			{
				return StatusCode(500, $"❌ Failed to retrieve summary: {ex.Message}");
			}
		}




	}
}
