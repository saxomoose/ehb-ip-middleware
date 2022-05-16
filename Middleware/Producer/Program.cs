using Microsoft.Net.Http.Headers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog();

builder.Services.AddControllers(options => {
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson();

// HttpClient
builder.Services.AddHttpClient("Wordpress", httpClient => {
    httpClient.BaseAddress = new Uri("http://frontend-wordpress-1/wp-json");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/json"
    );
});

builder.Services.AddHttpClient("UuidMasterApi", httpClient => {
    httpClient.BaseAddress = new Uri("http://uuidmasterapi-api-1:5000/api");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/json"
    );
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UsePathBase("/api");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
});

// app.Run(async (context) => {
//    await context.Response.WriteAsync("Hello from producer!");
// });

app.Run();