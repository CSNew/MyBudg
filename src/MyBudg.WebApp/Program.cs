using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using MyBudg.Business;
using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;
using MyBudg.WebApp.Components;
using MyBudg.WebApp.Components.Account;
using MyBudg.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// ---------- Data / Business (N-tier) ----------
// Database:Provider = Sqlite (ค่าเริ่มต้น เหมาะกับเครื่อง dev) หรือ Postgres (บนคลาวด์)
var provider = builder.Configuration.GetValue("Database:Provider", DatabaseProvider.Sqlite);
string connectionString;
if (provider == DatabaseProvider.Postgres)
{
    // รองรับทั้ง ConnectionStrings:Postgres และ DATABASE_URL (รูปแบบที่ Render/Neon/Supabase ให้มา)
    connectionString = builder.Configuration.GetConnectionString("Postgres")
        ?? builder.Configuration["DATABASE_URL"]
        ?? throw new InvalidOperationException("ใช้ Postgres แต่ยังไม่ได้ตั้ง ConnectionStrings__Postgres หรือ DATABASE_URL");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
}

// PaaS ส่งพอร์ตมาทาง PORT
if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
    builder.WebHost.UseUrls($"http://+:{port}");

// ใช้เฉพาะ docker compose/เครื่องทดลอง: ยอมต่อ Postgres แบบไม่ใช้ SSL ได้เมื่อโฮสต์เป็น localhost หรือชื่อ service ภายใน
var allowInsecureDb = builder.Configuration.GetValue<bool>("Database:AllowInsecureTransport");
builder.Services.AddMyBudgData(connectionString, EncryptionKeySetup.Load(builder.Configuration, builder.Environment), provider, allowInsecureDb);
builder.Services.Configure<RegistrationOptions>(builder.Configuration.GetSection("Registration"));

// เปิดเมื่ออยู่หลัง reverse proxy/load balancer ที่เป็นทางเข้าเดียวของแอปเท่านั้น
// (ถ้าแอปเข้าถึงได้ตรงๆ ผู้โจมตีจะปลอม X-Forwarded-For เพื่อหลบ rate limit ได้)
var behindProxy = builder.Configuration.GetValue<bool>("ForwardedHeaders:Enabled");
if (behindProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        o.ForwardLimit = 1; // เชื่อเฉพาะค่าที่ proxy ตัวเดียวหน้าแอปเติมให้ (ค่าขวาสุด) ที่ client ปลอมไม่ได้
        o.KnownIPNetworks.Clear();
        o.KnownProxies.Clear();
    });
}

builder.Services.AddMyBudgBusiness();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<AppState>();

// ---------- Authentication (ASP.NET Core Identity) ----------
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = IdentityConstants.ApplicationScheme;
        o.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(o =>
    {
        o.Password.RequiredLength = 12;
        o.Password.RequireDigit = true;
        o.Password.RequireLowercase = true;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequiredUniqueChars = 4;

        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        o.Lockout.AllowedForNewUsers = true;

        o.User.RequireUniqueEmail = true;
        // เปิดเป็น true เมื่อเสียบ IEmailSender<ApplicationUser> จริงแล้ว
        o.SignIn.RequireConfirmedAccount = false;
    })
    .AddMyBudgIdentityStores()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.Cookie.Name = "MyBudg.Auth";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromDays(7);
    o.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// จำกัดจำนวนครั้งการ POST หน้า Login/Register ต่อ IP กัน brute force
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("ลองบ่อยเกินไป กรุณารอสักครู่แล้วลองใหม่", ct);
    };
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var isAuthPost = HttpMethods.IsPost(ctx.Request.Method)
            && (ctx.Request.Path.StartsWithSegments("/Account/Login") || ctx.Request.Path.StartsWithSegments("/Account/Register"));
        if (!isAuthPost) return RateLimitPartition.GetNoLimiter("none");

        return RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) });
    });
});

var app = builder.Build();

if (allowInsecureDb && provider == DatabaseProvider.Postgres)
    app.Logger.LogWarning("Database:AllowInsecureTransport เปิดอยู่ — อนุญาตต่อ Postgres แบบไม่เข้ารหัสได้เฉพาะโฮสต์ภายใน ห้ามใช้กับระบบจริง");

if (behindProxy) app.UseForwardedHeaders();

// หลายอินสแตนซ์: ตั้ง Database:MigrateOnStartup=false แล้วรัน migration จาก pipeline แทน
if (builder.Configuration.GetValue("Database:MigrateOnStartup", true))
    await app.Services.InitializeMyBudgDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    var nonce = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
    ctx.Items["csp-nonce"] = nonce;
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "same-origin";
    h["Content-Security-Policy"] =
        $"default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src https://fonts.gstatic.com; img-src 'self' data:; connect-src 'self' ws: wss:; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});

// rate limiter ต้องอยู่นอกสุดเหนือ status-code-pages ไม่งั้น 429 ของ POST จะถูก re-execute
// ไปที่ /not-found (ยังเป็น POST) แล้วกลายเป็น 400
app.UseRateLimiter();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// หลัง proxy ที่ทำ TLS termination ให้ proxy เป็นผู้ redirect (และ health check จะเป็น http ภายใน)
if (!behindProxy) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();

// ตรวจสุขภาพ: ไม่เปิดเผยรายละเอียดใดๆ
app.MapGet("/healthz", async (IServiceProvider sp, CancellationToken ct) =>
    await sp.CanReachDatabaseAsync(ct) ? Results.Text("ok") : Results.StatusCode(StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

app.Run();

public partial class Program;
