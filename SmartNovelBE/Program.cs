using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartNovel.Services;
using SmartNovelBE.Models;
using SmartNovelBE.Services;
using SSmartNovelBE.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// 2. Database Context Configuration
var conString = builder.Configuration.GetConnectionString("SmartNovel");
builder.Services.AddDbContext<SmartTruyenDbContext>(options =>
    options.UseSqlServer(conString));

// 3. Cloudflare R2 (S3) Configuration
var r2Section = builder.Configuration.GetSection("CloudflareR2");
var accountId = r2Section["AccountId"];
var accessKey = r2Section["AccessKey"];
var secretKey = r2Section["SecretKey"];
var serviceUrl = $"https://{accountId}.r2.cloudflarestorage.com";

var credentials = new BasicAWSCredentials(accessKey, secretKey);
var config = new AmazonS3Config
{
    ServiceURL = serviceUrl,
};
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, config));

// 4. Authentication & JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // LỖI ĐƯỢC SỬA Ở ĐÂY: Thêm options.TokenValidationParameters = new TokenValidationParameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

// 5. Dependency Injection (Services)
builder.Services.AddScoped<JwtServices>();
builder.Services.AddScoped<INovelService, NovelService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<MailServices>();
builder.Services.AddSingleton<FileStorageServices>();

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

// 6. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// 7. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();