using System.Text;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using QLNS_KPI.Data;
using QLNS_KPI.Security;
using QLNS_KPI.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
var jwtKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured. Set it with user-secrets or an environment variable.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITemporaryPasswordEmailSender, SmtpTemporaryPasswordEmailSender>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("password-recovery", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "QLNS_KPI",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "QLNS_KPI_Frontend",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                var accountIdText = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var employeeIdText = principal?.FindFirst("employeeId")?.Value;
                if (!int.TryParse(accountIdText, out var accountId)
                    || !int.TryParse(employeeIdText, out var employeeId))
                {
                    context.Fail("Token claims are invalid.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var account = await db.Accounts.AsNoTracking()
                    .Where(x => x.Id == accountId)
                    .Select(x => new
                    {
                        x.EmployeeId,
                        RoleName = x.Role!.Name,
                        x.Status,
                        EmployeeStatus = x.Employee!.Status,
                        x.MustChangePassword
                    })
                    .SingleOrDefaultAsync(context.HttpContext.RequestAborted);
                if (account is null
                    || account.Status != "Hoạt động"
                    || account.EmployeeStatus != "Đang làm việc"
                    || account.EmployeeId != employeeId
                    || account.RoleName != principal!.FindFirstValue(ClaimTypes.Role))
                {
                    context.Fail("Account is no longer active.");
                    return;
                }

                if (account.MustChangePassword)
                {
                    var request = context.HttpContext.Request;
                    var isPasswordChangeRequest =
                        request.Method == HttpMethods.Post
                        && request.Path.Value?.Equals("/api/auth/change-password", StringComparison.OrdinalIgnoreCase) == true;
                    var isCurrentUserRequest =
                        request.Method == HttpMethods.Get
                        && request.Path.Value?.Equals("/api/auth/me", StringComparison.OrdinalIgnoreCase) == true;
                    if (!isPasswordChangeRequest && !isCurrentUserRequest)
                    {
                        context.Fail("Password change is required before using the application.");
                    }
                }
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
