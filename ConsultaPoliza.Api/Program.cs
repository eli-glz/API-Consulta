using ConsultaPoliza.Api.Options;
using ConsultaPoliza.Api.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<OraclePolicyOptions>(builder.Configuration.GetSection(OraclePolicyOptions.SectionName));

builder.Services.AddScoped<IReaGeneralPackage, ReaGeneralPackage>();
builder.Services.AddScoped<IPolicyResponseBuilder, PolicyResponseBuilder>();
builder.Services.AddScoped<IPolicyRepository, OraclePolicyRepository>();
builder.Services.AddScoped<IBranchRepository, OracleBranchRepository>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

app.MapControllers();

app.Run();
