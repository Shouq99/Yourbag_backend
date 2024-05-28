using System.Text;
using System.Text.Json.Serialization;
using ecommerce.EntityFramework;
using ecommerce.Middleware;
using ecommerce.service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Load environment variables from .env file
DotNetEnv.Env.Load();


// Get JWT settings from environment variables
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? throw new InvalidOperationException("JWT Issuer is missing in environment variables.");
var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? throw new InvalidOperationException("JWT Audience is missing in environment variables.");

// Get the database connection string from environment variables
var defaultConnection = Environment.GetEnvironmentVariable("DefaultConnection") ?? throw new InvalidOperationException("Default Connection is missing in environment variables.");



var Configuration = builder.Configuration;
var key = Encoding.ASCII.GetBytes(jwtKey);



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
    
});
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // set this one as a true in production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

// claim-based for banned user
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequiredNotBanned", policy => policy.RequireClaim("IsBanned", "false"));
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection)
);




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "E_commerce system API",
            Description =
                "Experience the future of online shopping today with E-commerce API. Designed to provide services for the system's users to manage products, and categories and engage customers with good experience. The goal is to deliver the tools needed to elevate a good e-commerce platform and drive incomparable success.",
            Version = "v1"
        }
    );
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});

builder.Services.AddControllers();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderItemService>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddAutoMapper(typeof(Program));

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers().WithParameterValidation();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Run the application
app.Run();

//qx1G6NsE4AiOVJWi