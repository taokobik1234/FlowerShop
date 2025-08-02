using System.Text;
using BackEnd_FLOWER_SHOP.Configuration;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Entities; 
using BackEnd_FLOWER_SHOP.Services;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using BackEnd_FLOWER_SHOP.Services.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Flower Shop API",
        Version = "v1",
        Description = "Flower Shop API with JWT Authentication. Click 'Authorize' button and enter your JWT token."
    });

    // Enhanced JWT Bearer configuration for Swagger with better UX
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter your token in the text input below.
                      Example: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
                      No need to add 'Bearer ' prefix - it will be added automatically."
    });

    // Apply JWT authentication globally to all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

});

builder.Services.AddControllers(); // Moved before builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Configure DbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Optional: Configure Identity options
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

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
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});
builder.Services.AddHttpContextAccessor();
// Register custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVnpay, Vnpay>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();

var app = builder.Build();
app.Urls.Add("https://localhost:5001");
app.Urls.Add("http://localhost:5000");

// START: Admin User Seeding Logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Ensure roles exist (as per your ApplicationDbContextModelSnapshot, "Admin" and "User" should be seeded)
        string adminRoleName = "Admin";
        string userRoleName = "User";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole(adminRoleName));
        }
        if (!await roleManager.RoleExistsAsync(userRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole(userRoleName));
        }

        // Create a default admin user if one doesn't exist
        var adminUserEmail = builder.Configuration["AdminSettings:Email"]; // Get from appsettings
        var adminUserPassword = builder.Configuration["AdminSettings:Password"]; // Get from appsettings
        var adminUserName = builder.Configuration["AdminSettings:UserName"]; // Get from appsettings

        if (string.IsNullOrEmpty(adminUserEmail) || string.IsNullOrEmpty(adminUserPassword) || string.IsNullOrEmpty(adminUserName))
        {
            // Fallback if appsettings are not configured for admin, or provide default
            adminUserEmail = "adminN@gmail.com";
            adminUserPassword = "adminnn1";
            adminUserName = "superadmin";
            Console.WriteLine("Warning: AdminSettings not found in appsettings.json or incomplete. Using default admin credentials.");
        }


        var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminUserEmail,
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true, // Set to true for quick testing, otherwise manage confirmation flow
                PhoneNumberConfirmed = true // Set to true for quick testing
            };

            var createAdminResult = await userManager.CreateAsync(adminUser, adminUserPassword);
            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
                Console.WriteLine($"Admin user '{adminUserEmail}' created and assigned '{adminRoleName}' role.");
            }
            else
            {
                Console.WriteLine("Failed to create admin user:");
                foreach (var error in createAdminResult.Errors)
                {
                    Console.WriteLine($"- {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine($"Admin user '{adminUserEmail}' already exists.");
            // Optionally, ensure the existing user has the Admin role
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
                Console.WriteLine($"Admin user '{adminUserEmail}' assigned '{adminRoleName}' role (was missing).");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// END: Admin User Seeding Logic

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flower Shop API V1");
        // Optional: Enable authorization persistence in Swagger UI
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication(); // Required for Identity
app.UseAuthorization(); // Required for Identity
app.MapControllers(); // Map controller routes

app.Run();