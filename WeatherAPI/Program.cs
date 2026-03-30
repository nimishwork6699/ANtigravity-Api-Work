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

app.Run();
