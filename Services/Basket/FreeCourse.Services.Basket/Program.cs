using System.IdentityModel.Tokens.Jwt;
using FreeCourse.Services.Basket.Services;
using FreeCourse.Services.Basket.Settings;
using FreeCourse.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["IdentityServerURL"];
    options.Audience = "resource_basket";
    options.RequireHttpsMetadata = false;
});

builder.Services.AddControllers(conf =>
{
    conf.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
});


builder.Services.AddHttpContextAccessor(); //farklı bir class içinden jwt içerisine erişmek için kullandık _httpContextAccessor.HttpContext.User.FindFirst("sub").Value;
builder.Services.AddScoped<ISharedIdentityService, SharedIdentityService>(); //jwt içindeki id ye her microserciveden daha rahat erişmek için kullandık

builder.Services.AddScoped<IBasketService, BasketService>();

builder.Services.AddSingleton<RedisService>(sp =>
{
    var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;

    var redis = new RedisService(redisSettings.Host, redisSettings.Port);

    redis.Connect();

    return redis;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();