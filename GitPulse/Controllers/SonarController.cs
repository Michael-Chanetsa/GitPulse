using GitPulse.Services;
using Microsoft.AspNetCore.Mvc;
using GitPulse.Models;
using GitPulse.Helpers;
using GitPulse.Data;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using System.Text.Json;

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
			[FromServices] SummaryService summaryService)
		{
			try
			{
				var waited = await _sonarService.WaitForAnalysisAsync(projectKey);
				if (!waited)
					return StatusCode(504, $"⏰ Timed out waiting for SonarQube analysis for {projectKey}");

				var metricsJson = await _sonarService.GetMetricsAsync(projectKey);
				var issuesJson = await _sonarService.GetTopIssuesAsync(projectKey);

				var report = SonarJsonParser.ParseReport(metricsJson, issuesJson);
				var summaryResult = summaryService.GenerateSummary(report);

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


		[HttpGet("scans")]
		public async Task<IActionResult> GetAllScans([FromServices] GitPulseDbContext db)
		{
			var scans = await db.ScanResults
				.OrderByDescending(s => s.ScanDate)
				.ToListAsync();

			return Ok(scans);
		}

		


	}
}
