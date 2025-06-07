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
			string token = "squ_44f1ecbd9adc6b648a85f81f00bca6da8e1e4f17";
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
			var response = await _http.GetAsync($"/api/issues/search?componentKeys={projectKey}&ps=5");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}
	}
}
