using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HealthChecks.Oracle;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Não registrar Oracle em ambiente de teste (será configurado pela CustomWebApplicationFactory)
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));
}

builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<PatioService>();
builder.Services.AddScoped<DispositivoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<PatioVision.Service.Services.ML.RedistribuicaoMLService>();
builder.Services.AddScoped<PatioVision.Service.Services.RedistribuicaoService>();
builder.Services.AddScoped<PatioVision.Data.Seeders.MlTrainingDataSeeder>();



builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); 
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "🏍️ PatioVision API",
        Version = "v1.0",
        Description = @"
## API para Rastreamento e Gerenciamento de Motocicletas

Sistema desenvolvido para uso exclusivo nos pátios da **Mottu**, permitindo o rastreamento e gerenciamento de motocicletas estacionadas em diferentes pátios através de dispositivos IoT.

### Características Principais
- 🎯 Rastreamento em tempo real através de dispositivos IoT
- 🤖 Machine Learning para recomendações de redistribuição de motos
- 🔐 Autenticação JWT segura
- 📊 Health checks para monitoramento
- 📝 Documentação completa via Swagger

### Autenticação
A maioria dos endpoints requer autenticação via Bearer Token. Para obter um token:
1. Crie um usuário através do endpoint `POST /api/v1/usuarios` (público)
2. Faça login através do endpoint `POST /api/v1/auth/login`
3. Use o token retornado no header `Authorization: Bearer {token}`
"
    });

    // Configurar autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"
JWT Authorization header usando o esquema Bearer. 
Digite 'Bearer' [espaço] e então seu token na caixa de texto abaixo.
Exemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Organizar endpoints por tags
    c.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Default";
        return new[] { controllerName };
    });

    c.DocInclusionPredicate((name, api) => true);

    // Incluir comentários XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurar ordem de exibição dos endpoints
    c.OrderActionsBy(apiDesc => 
    {
        var order = apiDesc.HttpMethod switch
        {
            "GET" => 1,
            "POST" => 2,
            "PUT" => 3,
            "PATCH" => 4,
            "DELETE" => 5,
            _ => 6
        };
        return $"{order}_{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}";
    });

    // Configurar schema IDs para evitar conflitos
    c.CustomSchemaIds(type => type.FullName);
});

builder.Services.AddHealthChecks()
    .AddOracle(
        connectionString: builder.Configuration.GetConnectionString("OracleConnection"),
        name: "Banco de dados Oracle",
        tags: new[] { "db", "oracle", "ready" })
    .AddCheck("API", () =>
        HealthCheckResult.Healthy("API funcionando normalmente"),  
        tags: new[] { "api", "live" });
    

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


var app = builder.Build();

// Habilitar Swagger em desenvolvimento e staging
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PatioVision API v1.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "PatioVision API - Documentação";
        c.DefaultModelsExpandDepth(-1); // Não expandir modelos por padrão
        c.DisplayRequestDuration(); // Mostrar tempo de requisição
        c.EnableDeepLinking(); // Permitir links diretos para endpoints
        c.EnableFilter(); // Habilitar filtro de busca
        c.ShowExtensions(); // Mostrar extensões
        c.EnableValidator(); // Habilitar validação
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // Expandir apenas lista
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}); 


app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

app.Run();

// Classe necessária para testes de integração com WebApplicationFactory
namespace PatioVision.API
{
    public partial class Program { }
}
