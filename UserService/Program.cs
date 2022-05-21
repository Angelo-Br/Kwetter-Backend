using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQLibrary;
using Serilog;
using System.Text;
using UserService.DBContexts;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "http://localhost:5001",
        ValidAudience = "http://localhost:5001",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("KwetterSecretJWTTokenForAuthentication"))
    };
});

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CorsPolicy",
        builder => builder.SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});


// Add all the logging components
var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build();

var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName().CreateLogger();

builder.Host.UseSerilog(logger);

// add controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService", Version = "v1" });
});

// add database services
builder.Services.AddDbContext<UserServiceDatabaseContext>();

using var userContext = new UserServiceDatabaseContext();
userContext.Database.EnsureCreated();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService v1"));
}



app.UseSerilogRequestLogging();

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

new Thread(() =>
{
    //ServiceCollectionExtensions.AddMessageConsuming("MailService");
    //new MessageConsumingFactory().AddMessageConsuming("MailService");
}).Start();


app.Run();
