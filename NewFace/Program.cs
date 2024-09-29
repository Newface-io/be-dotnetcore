using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewFace.Data;
using NewFace.Services;
using NewFace.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore-swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1_auth", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    c.SwaggerDoc("v1_home", new OpenApiInfo { Title = "Home API", Version = "v1" });
    c.SwaggerDoc("v1_user", new OpenApiInfo { Title = "User API", Version = "v1" });
    c.SwaggerDoc("v1_actor", new OpenApiInfo { Title = "Actor API", Version = "v1" });
    c.SwaggerDoc("v1_entertainment", new OpenApiInfo { Title = "Entertainment API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // 컨트롤러별로 API를 분리하기 위한 설정
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

        var controllerName = methodInfo.DeclaringType.Name.ToLower().Replace("controller", "");
        return docName.ToLower().Contains(controllerName);
    });

    c.EnableAnnotations();
});

// 다른 모든 곳에서 request context에 접근해야 할 때 필요
builder.Services.AddHttpContextAccessor();

// set CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://your-production-domain.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActorService, ActorService>();
builder.Services.AddScoped<IDockerFileService, DockerFileService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IMemoryManagementService, MemoryManagementService>(); // Redis 처리용 | 중앙에서 하나로 처리하므로 SingleTon으로 처리
builder.Services.AddScoped<ILogService, LogService>();

// DBContext
builder.Services.AddDbContext<DataContext>(options =>
{
    // 1. MSSQL
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // 2. MYSQL(MariaDB)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Redis
builder.Services.AddStackExchangeRedisCache(option =>
    option.Configuration = builder.Configuration.GetConnectionString("Cache"));

// JWT setting
string? secretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret Key is not configured.");
}

var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

DatabaseManagementService.MigrationInitialisation(app);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1_auth/swagger.json", "Auth");
    c.SwaggerEndpoint("/swagger/v1_home/swagger.json", "Home");
    c.SwaggerEndpoint("/swagger/v1_user/swagger.json", "User");
    c.SwaggerEndpoint("/swagger/v1_actor/swagger.json", "Actor");
    c.SwaggerEndpoint("/swagger/v1_entertainment/swagger.json", "Entertainment");
});

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

// upload image
app.UseStaticFiles(); // default wwwroot folder
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
//    RequestPath = "/uploads"
//});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();