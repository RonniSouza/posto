using Posto.Application;
using Posto.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

const string AngularDevCors = "AngularDev";
builder.Services.AddCors(options =>
    options.AddPolicy(AngularDevCors, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(AngularDevCors);
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
