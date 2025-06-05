namespace GitPulse.Models
{
	public class GitHubRepoRequestDTO
	{
		public string RepoUrl { get; set; } = string.Empty;
		public string? Token { get; set; } // optional for private repos
	}
}
