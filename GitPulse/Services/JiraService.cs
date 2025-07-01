using System.Text;
using System.Text.Json;
using GitPulse.Models;
using Microsoft.Extensions.Options;

namespace GitPulse.Services
{
	public class JiraService
	{
		private readonly HttpClient _httpClient;
		private readonly JiraConfiguration _config;
		private readonly ILogger<JiraService> _logger;

		public JiraService(IHttpClientFactory factory, IOptions<JiraConfiguration> config, ILogger<JiraService> logger)
		{
			_httpClient = factory.CreateClient("JiraClient");
			_config = config.Value;
			_logger = logger;

			ConfigureHttpClient();
		}

		private void ConfigureHttpClient()
		{
			string credentials = Convert.ToBase64String(
				Encoding.UTF8.GetBytes($"{_config.Email}:{_config.ApiToken}")
			);

			_httpClient.BaseAddress = new Uri(_config.BaseUrl);
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<JiraCreateResponse> CreateIssueAsync(JiraIssueDto dto)
		{
			try
			{
				var payload = new
				{
					fields = new
					{
						project = new { key = _config.ProjectKey },
						summary = dto.Summary,
						description = dto.Description,
						issuetype = new { name = dto.IssueType },
						priority = dto.Priority != null ? new { name = dto.Priority } : null,
						labels = dto.Labels ?? new List<string>()
					}
				};

				var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
				});

				var content = new StringContent(json, Encoding.UTF8, "application/json");

				_logger.LogInformation("Creating Jira issue with payload: {Payload}", json);

				var response = await _httpClient.PostAsync("/rest/api/3/issue", content);

				if (!response.IsSuccessStatusCode)
				{
					var error = await response.Content.ReadAsStringAsync();
					_logger.LogError("Jira issue creation failed: {StatusCode} - {Error}", response.StatusCode, error);
					throw new Exception($"Jira issue creation failed: {response.StatusCode}\n{error}");
				}

				var responseJson = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<JiraCreateResponse>(responseJson, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				_logger.LogInformation("Successfully created Jira issue: {IssueKey}", result?.Key);
				return result ?? new JiraCreateResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating Jira issue");
				throw;
			}
		}

		public async Task<bool> TestConnectionAsync()
		{
			try
			{
				var response = await _httpClient.GetAsync("/rest/api/3/myself");
				return response.IsSuccessStatusCode;
			}
			catch
			{
				return false;
			}
		}

		public JiraIssueDto CreateIssueFromSonarResult(SonarIssueDto sonarIssue, string repositoryName)
		{
			var severity = MapSeverityToPriority(sonarIssue.Severity);
			var issueType = MapSonarTypeToJiraType(sonarIssue.Type);

			return new JiraIssueDto
			{
				Summary = $"[{sonarIssue.Severity}] {sonarIssue.Message}",
				Description = CreateAtlassianDocumentFormat(sonarIssue, repositoryName),
				IssueType = issueType,
				Priority = severity,
				Labels = new List<string> { "sonar-qube", "code-review", repositoryName.ToLower() }
			};
		}

		private object CreateAtlassianDocumentFormat(SonarIssueDto issue, string repositoryName)
		{
			var contentItems = new List<object>
			{
				new
				{
					type = "paragraph",
					content = new[]
					{
						new { type = "text", text = "Auto-created from GitPulse code review", marks = new[] { new { type = "strong" } } }
					}
				},
				new
				{
					type = "paragraph",
					content = new[]
					{
						new { type = "text", text = $"Repository: {repositoryName}" }
					}
				},
				new
				{
					type = "paragraph",
					content = new[]
					{
						new { type = "text", text = $"SonarQube Rule: {issue.Rule}" }
					}
				}
			};

			// Add file path paragraph if it exists
			if (!string.IsNullOrEmpty(issue.FilePath))
			{
				contentItems.Add(new
				{
					type = "paragraph",
					content = new[]
					{
						new { type = "text", text = $"File: {issue.FilePath}" + (issue.Line.HasValue ? $" (Line: {issue.Line})" : "") }
					}
				});
			}

			// Add issue type paragraph
			contentItems.Add(new
			{
				type = "paragraph",
				content = new[]
				{
					new { type = "text", text = $"Issue Type: {issue.Type ?? "Unknown"}" }
				}
			});

			return new
			{
				type = "doc",
				version = 1,
				content = contentItems.ToArray()
			};
		}

		private string MapSeverityToPriority(string severity)
		{
			return severity?.ToUpper() switch
			{
				"BLOCKER" => "Highest",
				"CRITICAL" => "High",
				"MAJOR" => "Medium",
				"MINOR" => "Low",
				"INFO" => "Lowest",
				_ => "Medium"
			};
		}

		private string MapSonarTypeToJiraType(string? sonarType)
		{
			return sonarType?.ToUpper() switch
			{
				"BUG" => "Bug",
				"VULNERABILITY" => "Bug",
				"SECURITY_HOTSPOT" => "Task",
				"CODE_SMELL" => "Task",
				_ => "Task"
			};
		}
	}
}