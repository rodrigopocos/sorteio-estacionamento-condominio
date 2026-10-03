using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SorteioEstacionamento.Api;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;
using SorteioEstacionamento.Infrastructure;
using SorteioEstacionamento.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection("Admin"));
builder.Services.AddSingleton<IAdminCredenciais, AdminCredenciais>();

var connectionString = builder.Configuration.GetConnectionString("Sorteio") ?? "Data Source=sorteio-dev.db";
builder.Services.AddSorteioEstacionamento(connectionString);

var expireHours = builder.Configuration.GetValue("Auth:ExpireHours", 8);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "sorteio.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromHours(expireHours);
        o.SlidingExpiration = true;
        o.Events.OnRedirectToLogin = EscreverNaoAutenticado;
        o.Events.OnRedirectToAccessDenied = EscreverNaoAutenticado;
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<SorteioDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<FalhaRegraNegocioMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static Task EscreverNaoAutenticado(RedirectContext<CookieAuthenticationOptions> context)
{
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    return context.Response.WriteAsJsonAsync(new
    {
        codigo = CodigosErro.AuthRequired,
        mensagem = "Sessão necessária."
    });
}
