using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RestaurantWithAi.Core.Extensions;
using RestaurantWithAi.Shared.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AwsCognitoOptions>(
    builder.Configuration.GetSection("AWS"));
builder.Services.AddLogging();

var awsCognitoOptions = builder.Configuration.GetSection("AWS").Get<AwsCognitoOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = awsCognitoOptions.Authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = awsCognitoOptions.Authority,
            ValidateAudience = true,
            ValidAudience = awsCognitoOptions.ClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "cognito:groups",
            NameClaimType = "email",
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
