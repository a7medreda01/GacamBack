using AppBL.Helper;
using AppBL.IService;
using AppBL.Mapper;
using AppBL.Service;
using AppDAL.Context;
using AppDAL.IRepos;
using AppDAL.Repos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AppPL.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Repository / Unit of Work ───────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─── AutoMapper ──────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ─── Helpers ─────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileHelper, FileHelper>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();

// ─── Business Logic Services ─────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<IServiceFeeService, ServiceFeeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IMediaAccreditationService, MediaAccreditationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<ICertificateDesignService, CertificateDesignService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IAccreditationCategoryService, AccreditationCategoryService>();

// ─── JWT Authentication ──────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]
          ?? "GACAMSuperSecretSecurityKey2026!ForJWTAuthentication");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings["Issuer"] ?? "gacam.media",
        ValidateAudience         = true,
        ValidAudience            = jwtSettings["Audience"] ?? "gacam-client",
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─── Controllers ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Prevent circular reference errors (e.g. User → UserRoles → User)
        opts.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ─── CORS ────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── Swagger / OpenAPI ───────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "GACAM Backend API",
        Version     = "v1",
        Description = "Gulf & Arab General Commission for Audiovisual Media – Backend REST API. " +
                      "Includes Order Management, Profile Images, Pagination, Unified Verification, " +
                      "Accreditation Categories, and Certificate Expiration."
    });

    // Add JWT Bearer authorization to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your token below.\n\nExample: eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Auto-migrate + Seed ─────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Seed Roles + Admin user (needs BCrypt, available in AppPL scope)
    await AppPL.Extensions.ApplicationSeeder.SeedRolesAndAdminAsync(dbContext);
    // Seed 19 CMS Pages + Service Fees
    await DatabaseSeeder.SeedAsync(dbContext);
}

// ─── Middleware Pipeline ──────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GACAM API v1");
    });
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append(
            "Access-Control-Allow-Origin",
            "http://localhost:4200");

        ctx.Context.Response.Headers.Append(
            "Access-Control-Allow-Headers",
            "*");

        ctx.Context.Response.Headers.Append(
            "Access-Control-Allow-Methods",
            "*");
    }
});

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();

app.Run();
