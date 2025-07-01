using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GitPulse.Models;

namespace GitPulse.Services
{
	public class SonarService
	{
		private readonly HttpClient _http;

		public SonarService(IHttpClientFactory factory)
		{
			_http = factory.CreateClient();
			_http.BaseAddress = new Uri("http://localhost:9000");

			// Add the token as a Basic Auth header
			string token = "squ_b9b656a9ae4890822798af7424f2d2b547ed24d5";
			string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:"));
			_http.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", authHeader);
		}

		public async Task<string> GetMetricsAsync(string projectKey)
		{
			var metrics = "bugs,vulnerabilities,code_smells,coverage,duplicated_lines_density";
			var response = await _http.GetAsync($"/api/measures/component?component={projectKey}&metricKeys={metrics}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}

		public async Task<string> GetQualityGateStatusAsync(string projectKey)
		{
			var response = await _http.GetAsync($"/api/qualitygates/project_status?projectKey={projectKey}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}

		// return JSON string
		public async Task<string> GetTopIssuesAsync(string projectKey)
		{
			var response = await _http.GetAsync(
				$"/api/issues/search?componentKeys={projectKey}&types=BUG,CODE_SMELL,VULNERABILITY&ps=5");

			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}

		public async Task<bool> WaitForAnalysisAsync(string projectKey, int timeoutSeconds = 30)
		{
			var startTime = DateTime.UtcNow;

			while ((DateTime.UtcNow - startTime).TotalSeconds < timeoutSeconds)
			{
				var response = await _http.GetAsync($"/api/ce/component?component={projectKey}");
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(content);

				var root = doc.RootElement;
				if (root.TryGetProperty("queue", out var queue) && queue.GetArrayLength() == 0)
				{
					// ✅ Nothing in queue → processing finished
					return true;
				}

				await Task.Delay(3000); // wait 3 seconds before checking again
			}

			return false; // ⏰ Timeout
		}

	}
}
