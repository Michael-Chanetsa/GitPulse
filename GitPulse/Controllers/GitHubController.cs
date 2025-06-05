using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GitPulse.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using GitPulse.Services;

namespace GitPulse.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GitHubController : ControllerBase
	{

		/// <summary>
		/// The following Clones a GitHub repository to a temporary folder there after generates a temporary SonarQube project key and runs a SonarQube analysis on the cloned repository.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		/// 
		[HttpPost("analyze")]
		public async Task<IActionResult> AnalyzeRepo([FromBody] GitHubRepoRequestDTO request, [FromServices] SonarScannerService scanner)
		{
			if (string.IsNullOrWhiteSpace(request.RepoUrl))
				return BadRequest("RepoUrl is required.");

			string tempFolder = Path.Combine(Path.GetTempPath(), $"repo_{Guid.NewGuid()}");

			try
			{
				Directory.CreateDirectory(tempFolder);

				// 🔐 Handle private repo token if needed
				string cloneUrl = request.RepoUrl;
				if (!string.IsNullOrWhiteSpace(request.Token))
				{
					var uri = new Uri(request.RepoUrl);
					string tokenUrl = $"https://{request.Token}@{uri.Host}{uri.AbsolutePath}";
					cloneUrl = tokenUrl;
				}

				// 🧪 Clone the repo using git
				var cloneProcess = Process.Start(new ProcessStartInfo
				{
					FileName = "git",
					Arguments = $"clone {cloneUrl} \"{tempFolder}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				});

				await cloneProcess.WaitForExitAsync();

				if (cloneProcess.ExitCode != 0)
				{
					string error = await cloneProcess.StandardError.ReadToEndAsync();
					return StatusCode(500, $"Git clone failed:\n{error}");
				}

				// ✅ Clone successful — now run Sonar analysis
				string sonarToken = "squ_44f1ecbd9adc6b648a85f81f00bca6da8e1e4f17"; // your real Sonar token
				string sonarKey = $"scan_{Guid.NewGuid():N}"; // unique project key

				string resultKey = await scanner.RunSonarScanAsync(tempFolder, sonarKey, sonarToken);

				return Ok(new
				{
					message = "Repo cloned and analyzed successfully!",
					sonarKey = resultKey,
					metricsUrl = $"/api/sonar/metrics/{resultKey}"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Something went wrong: {ex.Message}");
			}
		}

	}
}
