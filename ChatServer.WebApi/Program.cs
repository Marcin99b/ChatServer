using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Consts;
using ChatServer.WebApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using ChatServer.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
builder.Services.SetupAuth(config);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<Repository>();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomsHub>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithOrigins("http://localhost:3000", "https://thunderous-biscotti-4a2236.netlify.app/"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Headers.ContainsKey(AuthConsts.AUTHORIZATION_HEADER))
    {
        ctx.Request.Headers.Remove(AuthConsts.AUTHORIZATION_HEADER);
    }

    if (ctx.Request.Cookies.ContainsKey(AuthConsts.ACCESS_TOKEN_COOKIE))
    {
        var authenticateResult = await ctx.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (authenticateResult.Succeeded && authenticateResult.Principal is not null)
        {
            ctx.User = authenticateResult.Principal;
        }
    }

    await next();
});

app.MapHub<RoomsHub>("/roomsHub");
app.MapControllers();


app.Run();