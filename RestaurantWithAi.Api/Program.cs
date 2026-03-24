using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RestaurantWithAi.Core.Extensions;
using RestaurantWithAi.Data.Extensions;
using RestaurantWithAi.Shared.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOptions<AwsCognitoOptions>()
    .Bind(builder.Configuration.GetSection(AwsCognitoOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddLogging();

var awsCognitoOptions = builder.Configuration.GetSection(AwsCognitoOptions.SectionName).Get<AwsCognitoOptions>()
                        ?? throw new InvalidOperationException($"Missing '{AwsCognitoOptions.SectionName}' configuration section.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = awsCognitoOptions.Authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = awsCognitoOptions.Authority,
            // Cognito access tokens use client_id (and may not carry aud in all flows).
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "cognito:groups",
            NameClaimType = "sub",
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var principal = context.Principal;
                var tokenUse = principal?.FindFirst("token_use")?.Value;
                var clientId = principal?.FindFirst("client_id")?.Value
                               ?? principal?.FindFirst("aud")?.Value;

                if (!string.Equals(tokenUse, "access", StringComparison.Ordinal) ||
                    !string.Equals(clientId, awsCognitoOptions.ClientId, StringComparison.Ordinal))
                {
                    context.Fail("Invalid access token.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = "sub";
});

builder.Services.AddAuthorization();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();
builder.Services.AddCoreServices();
builder.Services.AddDataServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
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
