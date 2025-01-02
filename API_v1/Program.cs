using API.ErrorHandling;
using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using Service;
using Service.Implement;
using System.Text;
using Service.EmailService;
using API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var emailConfig = builder.Configuration
        .GetSection("EmailConfiguration")
        .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddHttpClient("GHN", httpClient => {
    httpClient.BaseAddress = new Uri(builder.Configuration["API3rdParty:GHN:dev:url"]);
    httpClient.DefaultRequestHeaders.Add("token", builder.Configuration["API3rdParty:GHN:dev:token"]);
});
builder.Services.AddHttpClient("Zalopay", httpClient => {
    httpClient.BaseAddress = new Uri(builder.Configuration["API3rdParty:Zalopay:dev"]);
});
builder.Services.AddDbContext<PlatformAntiquesHandicraftsContext>(options => options.UseSqlServer("name=ConnectionStrings:dev"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add SignalR Hubs
builder.Services.AddSignalR();

//Add authorize to swagger
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
                Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
            }
        },
        new string[] {}}
    });
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowEverything", builder =>
    {
        builder.WithOrigins("*") // Replace with your allowed origins
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

//Filter
builder.Services.AddScoped<ValidateModelAttribute>();
builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.SuppressModelStateInvalidFilter = true;
});

//DI for scoped services
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<ITokenDAO, TokenDAO>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICategoryDAO, CategoryDAO>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMaterialDAO, MaterialDAO>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IProductDAO, ProductDAO>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuctionDAO, AuctionDAO>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IProductImageDAO, ProductImageDAO>();
builder.Services.AddScoped<IOrderCancelDAO, OrderCancelDAO>();
builder.Services.AddScoped<IOrderCancelService, OrderCancelService>();
builder.Services.AddScoped<IAddressDAO, AddressDAO>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IBuyerDAO, BuyerDAO>();
builder.Services.AddScoped<IOrderDAO, OrderDAO>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuctionDAO, AuctionDAO>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IBidDAO, BidDAO>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IJobTestService, JobTestService>();
builder.Services.AddScoped<ISellerDAO, SellerDAO>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IFeedbackDAO, FeedbackDAO>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IWalletDAO, WalletDAO>();
builder.Services.AddScoped<ITransactionDAO, TransactionDAO>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVerifyTokenDAO, VerifyTokenDAO>();
builder.Services.AddScoped<IWithdrawalDAO, WithdrawalDAO>();

//JWT authentication
builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters() {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents {
        OnAuthenticationFailed = context => {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException)) {
                context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
            }
            return Task.CompletedTask;
        }
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/auctionHub")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

//CORS policy
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(
        builder => {
            builder.WithOrigins("http://localhost:3000", "https://pah-administrator.vercel.app")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowAnyOrigin();
        });
});

//Hangfire Task scheduler
builder.Services.AddHangfire(x => {
    x.UseSqlServerStorage(builder.Configuration["ConnectionStrings:dev"]);
});
builder.Services.AddHangfireServer();

if (builder.Environment.IsDevelopment())
    builder.Services.AddHostedService<API.Tunnel.TunnelService>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler(logger);

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowEverything");

app.UseHangfireDashboard();

app.MapControllers();

app.MapHub<AuctionHub>("/auctionHub");

app.Run();
