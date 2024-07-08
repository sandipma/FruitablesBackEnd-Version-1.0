using Serilog;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FruitStoreRepositories.Implementation;
using FruitStoreRepositories.Interfaces;
using ExternalService.Implementation;
using ExternalService.Interfaces;
using FruitStoreRepositories.InterFaces;
using Serilog.Filters;
using Razorpay.Api;

public class Program
{
    private static int logFileCounter = 1; // Initialize counter
    public static async Task Main(string[] args)
    {
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(GetLogFilePath(), rollingInterval: RollingInterval.Infinite)
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting"))
            .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p.EndsWith("/swagger/index.html")))
            .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p.EndsWith("/_framework/aspnetcore-browser-refresh.js")))
            .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p.EndsWith("/_vs/browserLink")))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("User profile is available."))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Now listening on:"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Application started."))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Hosting environment:"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Content root path:"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Executing endpoint"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Route matched"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Executing ObjectResult"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Executed action"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Executed endpoint"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("Executing ObjectResult, writing value of type"))
            .Filter.ByExcluding(evt => evt.MessageTemplate.Text.Contains("FruitStoreModels.Response.ApiResponse`1[[FruitStoreModels.Auth.TokenDetails"))
            .CreateLogger();

        Log.Information("Starting Application \n");
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        try
        {
            // Your service configurations
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Fruits and vegetables API",
                    Description = "This API is used to sell fruits and vegetables",
                    Contact = new OpenApiContact
                    {
                        Name = "Sandip Mane",
                        Email = "sandipmane200@gmail.com",
                    },
                     
            });
               
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
               
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new List<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
            builder.Services.AddScoped<IHomeRepository, HomeRepository>();
            builder.Services.AddScoped<IShopRepository, ShopRepository>();
            builder.Services.AddScoped<IShopDetailsRepository, ShopDetailsRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IEmailService, EmailService>();    
            builder.Services.AddScoped<IAddressDetailsReposiotry, AddressDetailsReposiotry>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IAboutUsRepository, AboutUsRepository>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICartDetailsRepository, CartDetailsRepository>();
            builder.Services.AddSingleton(new RazorpayClient(builder.Configuration["RazorPay:Key"], builder.Configuration["RazorPay:Secret"]));
            builder.Services.AddScoped<IAdminOrderRepository, AdminOrderRepository>();

            builder.Services.AddHostedService(provider =>
            {
                using (var scope = provider.CreateScope()) 
                {
                    var scopedProvider = scope.ServiceProvider;
                    var cartRepository = scopedProvider.GetRequiredService<ICartDetailsRepository>();
                    var tokenRepository = scopedProvider.GetRequiredService<ITokenRepository>();
                    var logger = scopedProvider.GetRequiredService<ILogger<CronJobRepository>>();
                    return new CronJobRepository(cartRepository, tokenRepository, logger);
                }
            });

            var app = builder.Build();

            app.UseDeveloperExceptionPage();
          
            app.UseSwagger();
            app.UseSwaggerUI();

           
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            await app.RunAsync();


        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static string GetLogFilePath()
    {
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FruitablesServerLogs");
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var uniqueIdentifier = DateTime.Now.ToString("dd_MM_yyyy_HH-mm-ss");
        var logFileName = $"AppLog_{uniqueIdentifier}.txt";
        var logFilePath = Path.Combine(basePath, logFileName);

        return logFilePath;
    }

}
