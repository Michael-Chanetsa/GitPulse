//Db context for GitPulse application.
using Microsoft.EntityFrameworkCore;
using GitPulse.Models;

namespace GitPulse.Data
{
	public class GitPulseDbContext : DbContext
	{
		public GitPulseDbContext(DbContextOptions<GitPulseDbContext> options) : base(options) { }

		public DbSet<ScanResult> ScanResults { get; set; }
	}
}
