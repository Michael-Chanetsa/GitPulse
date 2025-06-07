using System.Text;
using System.Text.Json;
using GitPulse.Models;

namespace GitPulse.Services
{
	public class JiraService
	{
		private readonly HttpClient _http;

		public JiraService(IHttpClientFactory factory)
		{
			_http = factory.CreateClient();

			string email = "u21635634@tuks.co.za";
			string token = "ATATT3xFfGF0tbPJEOLDzUoJ58GT6PBaERPKmFYFNJagnIjvWU8jIml6CVqmW_XJrk7vsdEoBRTPGA9fhqfiRMavcxre42jJfyXg3hrMieY5dynFvg370dVC4VNyZ1GU-aCUkquaHzROsX-21zSgUBnNoxxgB8jPCVz2i6oegGZtKtgAwtwT-9k=87A810F5";
			string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{token}"));

			_http.BaseAddress = new Uri("https://inf370-team12.atlassian.net");
			_http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
		}

		public async Task<string> CreateIssueAsync(JiraIssueDto dto)
		{
			var payload = new
			{
				fields = new
				{
					project = new { key = "LEARNJIRA" },
					summary = dto.Summary,
					description = dto.Description,
					issuetype = new { name = dto.IssueType }
				}
			};

			var json = JsonSerializer.Serialize(payload);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _http.PostAsync("/rest/api/3/issue", content);

			if (!response.IsSuccessStatusCode)
			{
				var error = await response.Content.ReadAsStringAsync();
				throw new Exception($"Jira issue creation failed: {response.StatusCode}\n{error}");
			}

			return await response.Content.ReadAsStringAsync(); // Can return issue key/ID
		}
	}
}
