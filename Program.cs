using System.Text;
using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Middleware;
using EmployeeCrudPdf.Services;
using EmployeeCrudPdf.Services.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration values ----------
var jwtCfg = builder.Configuration.GetSection("Jwt");                  // << needed
var keyBytes = Encoding.UTF8.GetBytes(jwtCfg["Key"]!);                 // << needed
var signingKey = new SymmetricSecurityKey(keyBytes);                   // << needed

// ---------- Services / DI ----------
builder.Services.AddControllersWithViews();
// If you kept Razor Pages:
// builder.Services.AddRazorPages();

builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IAppLogger, FileLogger>();

builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".EmployeeCrudPdf.Session";
    o.IdleTimeout = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// ---------- AuthN / AuthZ ----------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtCfg["Issuer"],
            ValidAudience = jwtCfg["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                ctx.HandleResponse(); // stop default 401 body

                var path = ctx.Request.Path.Value ?? "";
                // Don’t redirect APIs, swagger or static
                if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/AccountMvc", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.CompletedTask;
                }

                var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                ctx.Response.Redirect($"/AccountMvc/Login?returnUrl={returnUrl}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EmployeeCrudPdf API",
        Version = "v1",
        Description = "Login/Register + per-user Employees, Products, Orders; pagination & search"
    });

    // XML docs (optional)
    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "access_token",
        Description = "JWT stored in cookie after /AccountMvc/Login"
    });
});

var app = builder.Build();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.DocumentTitle = "EmployeeCrudPdf API Docs";
        o.DisplayRequestDuration();
        o.EnablePersistAuthorization();
    });
}

app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseSession();
app.UseAuthentication();   // must be before UseAuthorization
app.UseAuthorization();

// Map MVC / API
app.MapControllers();

// If using Razor Pages:
// app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employees}/{action=Index}/{id?}");

app.Run();
