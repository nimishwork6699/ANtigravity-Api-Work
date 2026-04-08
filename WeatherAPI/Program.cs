var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add controllers to handle API requests
builder.Services.AddControllers();

// Add HttpClient to easily make requests to external APIs
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Enable routing and map the controller endpoints
app.MapControllers();

// Simple root and health endpoints to verify the app is running (useful for Azure checks)
//app.MapGet("/", () => Results.Json(new { status = "OK", service = "WeatherAPI" }));
//app.MapGet("/health", () => Results.Text("Healthy"));
app.MapGet("/", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();

    var response = await client.GetStringAsync("https://api.open-meteo.com/v1/forecast?latitude=51.5072&longitude=-0.1276&current_weather=true");

    return Results.Text(response, "application/json");
});

app.Run();
