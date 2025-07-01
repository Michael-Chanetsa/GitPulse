using System.Text;
using GitPulse.Data;
using GitPulse.Models;
using GitPulse.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ─────────────── Configuration ───────────────
var config = builder.Configuration;

// ─────────────── Database ───────────────
builder.Services.AddDbContext<GitPulseDbContext>(options =>
	options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// ─────────────── CORS ───────────────
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
	options.AddPolicy(MyAllowSpecificOrigins, policy =>
	{
		policy.WithOrigins("http://localhost:4200")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

// ─────────────── Jira & Groq Services ───────────────
builder.Services.Configure<JiraConfiguration>(config.GetSection("JiraConfiguration"));
builder.Services.AddHttpClient("JiraClient");

builder.Services.AddHttpClient(); // Generic fallback
builder.Services.AddScoped<SonarService>();
builder.Services.AddScoped<SonarScannerService>();
builder.Services.AddScoped<SummaryService>();
builder.Services.AddScoped<GroqService>(); // ✅ Groq AI service

// ─────────────── Authentication (JWT) ───────────────
builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
			ValidateIssuer = true,
			ValidIssuer = config["Jwt:Issuer"],
			ValidateAudience = true,
			ValidAudience = config["Jwt:Audience"],
			ValidateLifetime = true
		};
	});

builder.Services.AddAuthorization();

// ─────────────── Controllers & Swagger ───────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────── App Build & Pipeline ───────────────
var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
