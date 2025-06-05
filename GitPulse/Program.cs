using GitPulse.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Define CORS policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
		policy =>
		{
			policy.WithOrigins("http://localhost:4200") // Angular dev server
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddScoped<SonarService>();
builder.Services.AddScoped<SonarScannerService>();
builder.Services.AddScoped<SummaryService>();


var app = builder.Build();

// ✅ Enable CORS before anything else (important!)
app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
