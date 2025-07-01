using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GitPulse.Services
{
	public class GroqService
	{
		private readonly HttpClient _http;
		private readonly IConfiguration _config;
		private readonly ILogger<GroqService> _logger;

		public GroqService(IHttpClientFactory factory, IConfiguration config, ILogger<GroqService> logger)
		{
			_config = config;
			_logger = logger;
			_http = factory.CreateClient();

			// Set up the HTTP client for Groq
			_http.BaseAddress = new Uri("https://api.groq.com/");
			var apiKey = _config["Groq:ApiKey"];

			if (string.IsNullOrEmpty(apiKey))
			{
				throw new InvalidOperationException("Groq API key is not configured");
			}

			_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			_http.DefaultRequestHeaders.Add("User-Agent", "GitPulse/1.0");
		}

		public async Task<string> GetAdviceFromAiAsync(string prompt)
		{
			try
			{
				var requestBody = new
				{
					model = "deepseek-r1-distill-llama-70b",
					messages = new[]
					{
						new {
							role = "system",
							content = "You are an expert software development assistant. Provide clear, actionable advice for fixing code quality issues. Include specific solutions and best practices."
						},
						new {
							role = "user",
							content = prompt
						}
					},
					temperature = 0.7,
					max_tokens = 1000,
					top_p = 1,
					stream = false
				};

				var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				_logger.LogInformation("Sending request to Groq API: {Content}", jsonContent);

				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				// Use the correct endpoint
				var response = await _http.PostAsync("openai/v1/chat/completions", content);

				var responseContent = await response.Content.ReadAsStringAsync();
				_logger.LogInformation("Groq API response status: {StatusCode}, Content: {Content}",
					response.StatusCode, responseContent);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Groq API request failed with status {StatusCode}: {Content}",
						response.StatusCode, responseContent);
					throw new HttpRequestException($"Groq API request failed: {response.StatusCode} - {responseContent}");
				}

				using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
				var json = await JsonDocument.ParseAsync(responseStream);

				var choices = json.RootElement.GetProperty("choices");
				if (choices.GetArrayLength() == 0)
				{
					throw new InvalidOperationException("No response choices returned from Groq API");
				}

				var message = choices[0].GetProperty("message").GetProperty("content").GetString();
				return message ?? "No advice available";
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "Failed to parse JSON response from Groq API");
				throw new InvalidOperationException("Failed to parse response from AI service", ex);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "HTTP request to Groq API failed");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error calling Groq API");
				throw new InvalidOperationException("Failed to get advice from AI service", ex);
			}
		}
	}
}