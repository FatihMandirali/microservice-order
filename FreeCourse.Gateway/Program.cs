using FreeCourse.Gateway.DelegateHandlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile($"configuration.{builder.Environment.EnvironmentName.ToLower()}.json", true, true)
    .Build();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer("GatewayAuthenticationScheme", options =>
{
    options.Authority = builder.Configuration["IdentityServerURL"];
    options.Audience = "resource_gateway";
    options.RequireHttpsMetadata = false;
});

//builder.Services.AddOcelot(configuration).AddDelegatingHandler<TokenExhangeDelegateHandler>();

builder.Services.AddOcelot(configuration);
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

await app.UseOcelot();

//app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();