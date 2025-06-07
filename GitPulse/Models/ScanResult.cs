// Model to represent the result of a code scan. This is what will be saved in the database. Ask if I should add more properties like Name or something.
using System;
using System.ComponentModel.DataAnnotations;

namespace GitPulse.Models
{
	public class ScanResult
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string ProjectKey { get; set; } = "";
		public string RepoUrl { get; set; } = "";
		public DateTime ScanDate { get; set; } = DateTime.UtcNow;

		public int Bugs { get; set; }
		public int Vulnerabilities { get; set; }
		public int CodeSmells { get; set; }
		public double? Coverage { get; set; }
		public double? DuplicatedLinesDensity { get; set; }

		public string Summary { get; set; } = "";
		public string QualityGate { get; set; } = "";
	}
}
