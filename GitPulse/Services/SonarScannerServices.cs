using System.Diagnostics;

namespace GitPulse.Services
{
	public class SonarScannerService
	{
		public async Task<string> RunSonarScanAsync(string repoPath, string sonarProjectKey, string token)
		{
			// 🧠 Auto-detect project type
			bool isDotNet = Directory.GetFiles(repoPath, "*.sln", SearchOption.AllDirectories).Any();
			bool isNode = File.Exists(Path.Combine(repoPath, "package.json"));
			bool isPython = Directory.GetFiles(repoPath, "*.py", SearchOption.AllDirectories).Any();

			if (isDotNet)
				return await RunDotNetSonarScanAsync(repoPath, sonarProjectKey, token);
			else
				return await RunGenericSonarScanAsync(repoPath, sonarProjectKey, token);
		}

		private async Task<string> RunDotNetSonarScanAsync(string repoPath, string sonarProjectKey, string token)
		{
			async Task RunStep(string command, string args, string workingDir)
			{
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = command,
						Arguments = args,
						WorkingDirectory = workingDir,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				process.Start();

				string output = await process.StandardOutput.ReadToEndAsync();
				string error = await process.StandardError.ReadToEndAsync();

				await process.WaitForExitAsync();

				if (process.ExitCode != 0)
				{
					throw new Exception($"❌ Command `{command} {args}` failed.\n\nOUTPUT:\n{output}\n\nERROR:\n{error}");
				}
			}

			// TEMP FIX: Remove global.json if exists
			var globalJsonPath = Path.Combine(repoPath, "global.json");
			if (File.Exists(globalJsonPath))
				File.Delete(globalJsonPath);

			var solutionPath = Directory.GetFiles(repoPath, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
			if (solutionPath == null)
				throw new FileNotFoundException("❌ No .sln file found in the cloned repository.");

			var solutionDir = Path.GetDirectoryName(solutionPath)!;

			string sonarBegin = $"sonarscanner begin /k:\"{sonarProjectKey}\" /d:sonar.login=\"{token}\" /d:sonar.host.url=\"http://localhost:9000\"";
			string sonarEnd = $"sonarscanner end /d:sonar.login=\"{token}\"";

			await RunStep("dotnet", sonarBegin, solutionDir);
			await RunStep("dotnet", $"build \"{solutionPath}\"", solutionDir);
			await RunStep("dotnet", sonarEnd, solutionDir);

			return sonarProjectKey;
		}

		private async Task<string> RunGenericSonarScanAsync(string repoPath, string sonarProjectKey, string token)
		{
			// 1. Create sonar-project.properties file
			string props = $@"
				sonar.projectKey={sonarProjectKey}
				sonar.sources=.
				sonar.host.url=http://localhost:9000
				sonar.login={token}
				";

			string propPath = Path.Combine(repoPath, "sonar-project.properties");
			await File.WriteAllTextAsync(propPath, props);

			// 2. Run sonar-scanner CLI
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = @"C:\Users\chane\Desktop\SonarQube\sonar-scanner-7.1.0.4889-windows-x64\bin\sonar-scanner.bat",
					WorkingDirectory = repoPath,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			// Ensure the path to sonar-scanner.bat is correct
			process.Start();

			string output = await process.StandardOutput.ReadToEndAsync();
			string error = await process.StandardError.ReadToEndAsync();

			await process.WaitForExitAsync();

			if (process.ExitCode != 0)
				throw new Exception($"❌ sonar-scanner failed.\n\nOUTPUT:\n{output}\n\nERROR:\n{error}");

			return sonarProjectKey;
		}
	}
}
