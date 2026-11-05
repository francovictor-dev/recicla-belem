using Amazon;
using Amazon.S3;
using Backend.Configurations;
using Microsoft.Extensions.Options;
using Backend.Data;
using Backend.Data.Seeders;
using Backend.Filters;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Configuração de autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Add serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
  options.AddPolicy("FrontendDevelopment", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());

  options.AddPolicy("Frontend", policy =>
    policy.WithOrigins("https://recicla-belem.vercel.app").AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddOptions<S3Options>()
    .Configure(options =>
    {
      options.Region = builder.Configuration["AWS_REGION"] ?? builder.Configuration["S3:Region"] ?? "";
      options.Bucket = builder.Configuration["AWS_BUCKET"] ?? builder.Configuration["S3:Bucket"] ?? "";
      options.PublicUrl = builder.Configuration["AWS_PUBLIC_URL"] ?? builder.Configuration["S3:PublicUrl"] ?? "";
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.Region), "Configure a região do S3.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Bucket), "Configure o bucket do S3.")
    .Validate(options => Uri.TryCreate(options.PublicUrl, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps,
        "Configure uma URL HTTPS para o S3.");
builder.Services.AddSingleton<IAmazonS3>(provider =>
    new AmazonS3Client(RegionEndpoint.GetBySystemName(provider.GetRequiredService<IOptions<S3Options>>().Value.Region)));
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExternalLoginService, ExternalLoginService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<ICollectorService, CollectorService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("Nominatim", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["Nominatim:BaseUrl"] ?? "https://nominatim.openstreetmap.org/");
  client.DefaultRequestHeaders.UserAgent.ParseAdd("ReciclaBelem/1.0");
  client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<INominatimService>(provider => new NominatimService(
    provider.GetRequiredService<IHttpClientFactory>().CreateClient("Nominatim"),
    provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
    provider.GetRequiredService<ILogger<NominatimService>>()));
builder.Services.AddScoped<ICollectionPointService, CollectionPointService>();
builder.Services.AddScoped<ICollectionPointCollectorService, CollectionPointCollectorService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped(typeof(OwnershipFilter<>));
builder.Services.AddScoped<CoordinateDebugFilter>();

builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Digite: Bearer {seu token}"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(
      builder.Configuration.GetConnectionString("DefaultConnection"),
      sqlOptions => sqlOptions.EnableRetryOnFailure(
          maxRetryCount: 10,
          maxRetryDelay: TimeSpan.FromSeconds(15),
          errorNumbersToAdd: null
      )
  )
);

// LOCAL
/* builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))); */

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.MapInboundClaims = false;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtIssuer,
    ValidAudience = jwtAudience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    ClockSkew = TimeSpan.Zero // sem tolerância extra na expiração
  };
});

// builder.Services.AddAuthorization(options =>
// {
//   options.AddPolicy("Ownership", policy => policy.Requirements.Add(new OwnershipRequirement()));
// });
builder.Services.AddAuthorization();

Console.WriteLine(
    $"AWS_ACCESS_KEY_ID preenchida: {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"))}");

Console.WriteLine(
    $"AWS_SECRET_ACCESS_KEY preenchida: {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"))}");

var app = builder.Build();

/* await using (var scope = app.Services.CreateAsyncScope())
{
  var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  await CollectorSeeder.SeedAsync(context, logger: app.Logger);
  await AdminUserSeeder.SeedAsync(context, app.Configuration, logger: app.Logger);
} */

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
  using var scope = app.Services.CreateScope();

  var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

  await context.Database.MigrateAsync();

  await CollectorSeeder.SeedAsync(context, logger: app.Logger);
  await AdminUserSeeder.SeedAsync(context, app.Configuration, logger: app.Logger);
}

// Configuração dos middlewares
app.UseRouting();

// only development environment should use swagger and https redirection, production should not use it because it is behind a reverse proxy that handles https redirection
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseHttpsRedirection();
  app.UseCors("FrontendDevelopment");
}
else if (app.Environment.IsProduction())
{
  app.UseCors("Frontend");
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// TEST
app.MapGet("/health", () =>
{
  return Results.Ok(new
  {
    status = "healthy",
    timestamp = DateTime.UtcNow
  });
}).AllowAnonymous();

app.Run();

public partial class Program { }