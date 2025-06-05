using System.Diagnostics;
using System.Text.Json;

namespace GitPulse.Helpers
{
	public static class DotnetSdkHelper
	{
		public static string? GetSdkVersionFromGlobalJson(string repoPath)
		{
			var globalJsonPath = Path.Combine(repoPath, "global.json");
			if (!File.Exists(globalJsonPath))
				return null;

			var json = File.ReadAllText(globalJsonPath);
			using var doc = JsonDocument.Parse(json);

			if (doc.RootElement.TryGetProperty("sdk", out var sdkObj) &&
				sdkObj.TryGetProperty("version", out var versionElement))
			{
				return versionElement.GetString();
			}

			return null;
		}

		public static List<string> GetInstalledDotnetSdks()
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "dotnet",
					Arguments = "--list-sdks",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			var sdks = new List<string>();
			foreach (var line in output.Split('\n'))
			{
				var version = line.Split(' ').FirstOrDefault()?.Trim();
				if (!string.IsNullOrEmpty(version))
					sdks.Add(version);
			}

			return sdks;
		}

		public static bool IsSdkVersionInstalled(string? requiredVersion)
		{
			if (string.IsNullOrWhiteSpace(requiredVersion)) return true;
			var installed = GetInstalledDotnetSdks();
			return installed.Any(v => v.StartsWith(requiredVersion));
		}
	}
}
