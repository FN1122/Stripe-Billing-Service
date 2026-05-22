using Core.ContextProviders;
using Core.ErrorHandling;
using Core.Infrastructure;
using Core.Utils;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StripeBilling.API.BackgroundServices;
using StripeBilling.API.Hubs;
using StripeBilling.API.Middleware;
using StripeBilling.API.Utils;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Controllers
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ValidationErrorResponseFactory.CreateResponse;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// CORS
builder.Services.AddCors(options =>
{
    // ✅ Allow ALL origins/headers/methods (DEV-friendly).
    // NOTE: AllowAnyOrigin cannot be combined with AllowCredentials.
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    // Named policy (optional) if you prefer: app.UseCors("AllowAll")
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Problem Details
builder.Services.AddProblemDetails();

// Tenant Context Provider
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContextProvider, HttpTenantContextProvider>();

// Database
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=StripeBillingDb;Trusted_Connection=true;TrustServerCertificate=true;",
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)));

// Data Protection
builder.Services.AddDataProtection();

// Memory Cache
builder.Services.AddMemoryCache();

// HTTP Clients
builder.Services.AddHttpClient("Stripe", client =>
{
    client.BaseAddress = new Uri("https://api.stripe.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddHttpClient("Webhook", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// SignalR
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// FluentValidation - auto-validation on model binding + register all validators from Core assembly
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.Load("StripeBilling.Core"));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32Characters!StripeBilling2026";
builder.Services.AddAuthentication(options =>
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
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "StripeBilling",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "StripeBilling",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // SignalR JWT support via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("AdminOrAbove", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("Admin", "Manager", "Viewer"));
});

// Auto-register services and repositories
builder.Services.RegisterServiceLayerDi();
builder.Services.RegisterRepositoryLayerDi();

// Background services
builder.Services.AddHostedService<WebhookDispatcherService>();
builder.Services.AddHostedService<WebhookRetryService>();
builder.Services.AddHostedService<DunningProcessorService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Stripe Billing Service API",
        Version = "v1.0",
        Description = "Multi-tenant Stripe Billing & Subscription Management REST API.\n\n" +
            "## Getting Started\n" +
            "1. Call **POST /api/v1/auth/login** with email & password to get a JWT token\n" +
            "2. Click the **Authorize** button and paste the token\n" +
            "3. Include the **X-Tenant-Id** header for tenant-scoped endpoints\n\n" +
            "## Demo Credentials (All passwords: Demo@123)\n" +
            "| Role | Email |\n" +
            "|------|-------|\n" +
            "| SuperAdmin | superadmin@techflow.com |\n" +
            "| Admin | sarah@techflow.com |\n" +
            "| Manager | ahmed@techflow.com |\n" +
            "| Viewer | viewer@techflow.com |\n" +
            "| Admin (Tenant 2) | admin@sunrisedental.com |\n" +
            "| Manager (Tenant 2) | billing@sunrisedental.com |\n\n" +
            "## Tenant IDs\n" +
            "Tenant IDs are auto-generated at startup. Log in and check the JWT token payload for your tenant ID.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Muhammad Nasir",
            Email = "yousafzainasir9@gmail.com"
        }
    });

    // Tag descriptions for logical grouping
    c.TagActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"];
        return controller switch
        {
            "Auth" => ["1. Authentication"],
            "Customer" => ["2. Customers"],
            "Payment" => ["3. Payments"],
            "Subscription" => ["4. Subscriptions"],
            "Plan" => ["5. Plans"],
            "Invoice" => ["6. Invoices"],
            "Refund" => ["7. Refunds"],
            "Coupon" => ["8. Coupons"],
            "UsageBilling" => ["9. Usage Billing"],
            "Analytics" => ["10. Analytics"],
            "Dashboard" => ["11. Dashboard"],
            "Webhook" => ["12. Webhooks"],
            "WebhookInbound" => ["13. Stripe Webhooks"],
            "WebhookEvent" => ["14. Webhook Events"],
            "ApiKey" => ["15. API Keys"],
            "Log" => ["16. Logs"],
            "Dunning" => ["17. Dunning"],
            "Tax" => ["18. Tax"],
            "Email" => ["19. Email Templates"],
            "Credit" => ["20. Credits"],
            "Connect" => ["21. Stripe Connect"],
            "Export" => ["22. Exports"],
            "User" => ["23. User Management"],
            "Settings" => ["24. Settings"],
            "RateLimit" => ["25. Rate Limiting"],
            "Audit" => ["26. Audit Logs"],
            "SuperAdmin" => ["27. Super Admin"],
            "Portal" => ["28. Customer Portal"],
            "Health" => ["29. Health Check"],
            _ => [controller ?? "Other"]
        };
    });

    c.DocInclusionPredicate((_, _) => true);

    // JWT Bearer auth
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste your JWT token from the /api/auth/login response.\n\nExample: eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // X-Tenant-Id global header (shown on every endpoint)
    c.OperationFilter<SwaggerTenantHeaderFilter>();

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stripe Billing Service API v1.0");
    c.DocumentTitle = "Stripe Billing API - Swagger";
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
    c.DefaultModelsExpandDepth(1);
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.EnableFilter();

    // Custom CSS for branding
    c.HeadContent = @"
        <style>
            .swagger-ui .topbar { background-color: #1A56DB; }
            .swagger-ui .topbar .download-url-wrapper .select-label { color: #fff; }
            .swagger-ui .info .title { color: #1A56DB; }
            .swagger-ui .opblock-tag { font-size: 16px; border-bottom: 1px solid #e0e0e0; }
            .swagger-ui .opblock.opblock-post { border-color: #059669; background: rgba(5, 150, 105, 0.05); }
            .swagger-ui .opblock.opblock-get { border-color: #1A56DB; background: rgba(26, 86, 219, 0.05); }
            .swagger-ui .opblock.opblock-put { border-color: #D97706; background: rgba(217, 119, 6, 0.05); }
            .swagger-ui .opblock.opblock-delete { border-color: #DC2626; background: rgba(220, 38, 38, 0.05); }
        </style>";
});


// ✅ Redirect "/" to "/swagger"
app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// Global exception handler (inline middleware)
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (FluentValidation.ValidationException vex)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var response = new GatewayResponseWrapper<object>();
            response.SetError(vex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.", 400);
            response.Errors = vex.Errors.Select(e => e.ErrorMessage).ToList();
            await context.Response.WriteAsJsonAsync(response);
        }
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Unhandled exception: {Message}", ex.Message);
        try
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var response = new GatewayResponseWrapper<object>();
                response.SetError("An unexpected error occurred.", 500);
                await context.Response.WriteAsJsonAsync(response);
            }
        }
        catch (ObjectDisposedException) { /* Response stream already closed */ }
    }
});

app.UseCors();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

// ✅ Don't require API key / HMAC / tenant / idempotency for Swagger & health
app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/swagger")
        && !ctx.Request.Path.StartsWithSegments("/health"),
    branch =>
    {
        branch.UseMiddleware<ApiKeyAuthMiddleware>();
        branch.UseMiddleware<HmacAuthMiddleware>();
        branch.UseMiddleware<TenantMiddleware>();
        branch.UseMiddleware<IdempotencyMiddleware>();
    });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    try
    {
        // Drop & recreate database for fresh seed data
        // Close other connections (e.g. SSMS) to the database first to avoid "in use" errors
        try
        {
            db.Database.EnsureDeleted();
        }
        catch (Exception delEx)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  [Seed] Warning: Could not delete database ({delEx.Message}).");
            Console.WriteLine("  [Seed] Close SSMS and other connections, then restart the app for a fresh seed.");
            Console.ResetColor();
        }
        db.Database.EnsureCreated();

        if (!db.Tenants.Any())
        {
            // ─── Tenants ───────────────────────────────────────────────
            var tenantId = Guid.NewGuid();
            var tenant = new Core.Infrastructure.Tenant
            {
                Id = tenantId,
                Name = "TechFlow Solutions",
                Slug = "techflow",
                Description = "SaaS platform for developer tools and API management",
                PublicApiKey = "",
                PublicKey = "",
                SecretApiKeyHash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(""))),
                WebhookSigningSecret = "whsec_tfl_a3b8c2d9e4f7g1h6i5j0k",
                WebhookCallbackUrl = "https://api.techflow.io/webhooks/stripe",
                JwtSigningSecret = "jwt_techflow_secret_key_that_is_at_least_32_characters_long_2026",
                StripeSecretKeyEnc = "",
                StripePublishableKey = "",
                StripeWebhookSecret = "",
                Plan = "enterprise",
                Settings = "{\"timezone\":\"America/New_York\",\"invoicePrefix\":\"TFL\",\"defaultCurrency\":\"usd\",\"paymentRetryDays\":7}",
                Features = "[\"multi_currency\",\"usage_billing\",\"dunning\",\"stripe_connect\",\"custom_webhooks\",\"api_access\",\"sla_guarantee\"]",
                Metadata = "{\"industry\":\"Technology\",\"website\":\"https://techflow.io\",\"foundedYear\":2021}",
                SuspensionReason = "",
                IsActive = true
            };
            db.Tenants.Add(tenant);

            var tenant2Id = Guid.NewGuid();
            var tenant2 = new Core.Infrastructure.Tenant
            {
                Id = tenant2Id,
                Name = "Sunrise Dental Group",
                Slug = "sunrise-dental",
                Description = "Multi-location dental practice management and patient billing",
                PublicApiKey = "",
                PublicKey = "",
                SecretApiKeyHash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(""))),
                WebhookSigningSecret = "whsec_sdn_x7y2z8a3b4c9d0e5f1g6h",
                WebhookCallbackUrl = "https://billing.sunrisedental.com/webhooks",
                JwtSigningSecret = "jwt_sunrise_dental_secret_key_32_characters_minimum_2026",
                StripeSecretKeyEnc = "",
                StripePublishableKey = "",
                StripeWebhookSecret = "",
                Plan = "professional",
                Settings = "{\"timezone\":\"America/Chicago\",\"invoicePrefix\":\"SDG\",\"defaultCurrency\":\"usd\",\"paymentRetryDays\":5}",
                Features = "[\"invoicing\",\"dunning\",\"email_templates\",\"tax_management\"]",
                Metadata = "{\"industry\":\"Healthcare\",\"website\":\"https://sunrisedental.com\",\"locations\":3}",
                SuspensionReason = "",
                IsActive = true
            };
            db.Tenants.Add(tenant2);
            db.SaveChanges();
            Console.WriteLine("  [Seed] Tenants created.");

            // ─── Users ─────────────────────────────────────────────────
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123", 12);

            var superAdminUser = new Core.Infrastructure.User
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Email = "superadmin@techflow.com", PasswordHash = passwordHash,
                FullName = "Muhammad Nasir", FirstName = "Muhammad", LastName = "Nasir",
                Role = "SuperAdmin", Permissions = "[\"*\"]",
                Metadata = "{\"department\":\"Engineering\",\"title\":\"CTO & Co-Founder\"}",
                IsActive = true, LastLoginAt = DateTime.UtcNow.AddHours(-2)
            };
            var adminUser = new Core.Infrastructure.User
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Email = "sarah@techflow.com", PasswordHash = passwordHash,
                FullName = "Sarah Mitchell", FirstName = "Sarah", LastName = "Mitchell",
                Role = "Admin", Permissions = "[\"billing\",\"customers\",\"subscriptions\",\"invoices\",\"analytics\",\"users\",\"settings\"]",
                Metadata = "{\"department\":\"Finance\",\"title\":\"VP of Finance\"}",
                IsActive = true, LastLoginAt = DateTime.UtcNow.AddHours(-5)
            };
            var managerUser = new Core.Infrastructure.User
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Email = "ahmed@techflow.com", PasswordHash = passwordHash,
                FullName = "Ahmed Khalil", FirstName = "Ahmed", LastName = "Khalil",
                Role = "Manager", Permissions = "[\"billing\",\"customers\",\"subscriptions\",\"invoices\"]",
                Metadata = "{\"department\":\"Billing Operations\",\"title\":\"Billing Manager\"}",
                IsActive = true, LastLoginAt = DateTime.UtcNow.AddDays(-1)
            };
            var viewerUser = new Core.Infrastructure.User
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                Email = "viewer@techflow.com", PasswordHash = passwordHash,
                FullName = "Lisa Chen", FirstName = "Lisa", LastName = "Chen",
                Role = "Viewer", Permissions = "[]",
                Metadata = "{\"department\":\"Support\",\"title\":\"Customer Success Rep\"}",
                IsActive = true, LastLoginAt = DateTime.UtcNow.AddDays(-3)
            };
            db.Users.AddRange(superAdminUser, adminUser, managerUser, viewerUser);

            // Tenant 2 users
            db.Users.Add(new Core.Infrastructure.User
            {
                Id = Guid.NewGuid(), TenantId = tenant2Id,
                Email = "admin@sunrisedental.com", PasswordHash = passwordHash,
                FullName = "Dr. Emily Nguyen", FirstName = "Emily", LastName = "Nguyen",
                Role = "Admin", Permissions = "[\"billing\",\"customers\",\"invoices\",\"settings\"]",
                Metadata = "{\"department\":\"Administration\",\"title\":\"Practice Owner\"}",
                IsActive = true, LastLoginAt = DateTime.UtcNow.AddHours(-8)
            });
            db.Users.Add(new Core.Infrastructure.User
            {
                Id = Guid.NewGuid(), TenantId = tenant2Id,
                Email = "billing@sunrisedental.com", PasswordHash = passwordHash,
                FullName = "Rachel Torres", FirstName = "Rachel", LastName = "Torres",
                Role = "Manager", Permissions = "[\"billing\",\"customers\",\"invoices\"]",
                Metadata = "{\"department\":\"Billing\",\"title\":\"Billing Coordinator\"}",
                IsActive = true, LastLoginAt = DateTime.UtcNow.AddDays(-1)
            });

            db.SaveChanges();
            Console.WriteLine("  [Seed] Users created.");

            // ─── API Keys ──────────────────────────────────────────────
            db.ApiKeys.Add(new Core.Infrastructure.ApiKey
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                KeyHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(""))),
                KeyEnc = "", KeyPrefix = "pk__51Ox", Name = "Production Public Key",
                Description = "Primary public API key for TechFlow frontend integration", Environment = "",
                Permissions = "[\"payments\",\"subscriptions\",\"customers\",\"invoices\",\"checkout\"]",
                RateLimitPerMinute = 120, TotalRequests = 48723, LastUsedAt = DateTime.UtcNow.AddMinutes(-15)
            });
            db.ApiKeys.Add(new Core.Infrastructure.ApiKey
            {
                Id = Guid.NewGuid(), TenantId = tenantId,
                KeyHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(""))),
                KeyEnc = "", KeyPrefix = "sk__51Ox", Name = "Backend Secret Key",
                Description = "Server-side secret key for backend API operations", Environment = "",
                Permissions = "[\"payments\",\"subscriptions\",\"customers\",\"invoices\",\"refunds\",\"webhooks\",\"analytics\"]",
                RateLimitPerMinute = 200, TotalRequests = 156892, LastUsedAt = DateTime.UtcNow.AddMinutes(-3)
            });
            db.ApiKeys.Add(new Core.Infrastructure.ApiKey
            {
                Id = Guid.NewGuid(), TenantId = tenant2Id,
                KeyHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(""))),
                KeyEnc = "", KeyPrefix = "pk__82Gh", Name = "Sunrise Dental Public Key",
                Description = "Public API key for patient portal", Environment = "",
                Permissions = "[\"payments\",\"customers\",\"invoices\"]",
                RateLimitPerMinute = 60, TotalRequests = 12450, LastUsedAt = DateTime.UtcNow.AddHours(-1)
            });

            // ─── Subscription Plans ────────────────────────────────────
            var starterPlan = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Starter", Description = "Perfect for solo developers and small side projects", StripePriceId = "price_1OxQrBCz3m7kX9aL_starter", StripeProductId = "prod_starter_techflow", Amount = 9.00m, Currency = "usd", Interval = "month", TrialDays = 14, Features = "[\"Up to 3 users\",\"1,000 API calls/month\",\"Email support\",\"1GB storage\",\"Basic analytics\"]", SortOrder = 1 };
            var proPlan = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Professional", Description = "Ideal for growing teams and scaling businesses", StripePriceId = "price_1OxQrBCz3m7kX9aL_pro", StripeProductId = "prod_pro_techflow", Amount = 49.00m, Currency = "usd", Interval = "month", TrialDays = 14, Features = "[\"Up to 25 users\",\"50,000 API calls/month\",\"Priority email & chat support\",\"25GB storage\",\"Advanced analytics\",\"Webhook integrations\",\"Custom branding\"]", SortOrder = 2 };
            var businessPlan = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Business", Description = "For established companies with advanced needs", StripePriceId = "price_1OxQrBCz3m7kX9aL_biz", StripeProductId = "prod_biz_techflow", Amount = 149.00m, Currency = "usd", Interval = "month", TrialDays = 30, Features = "[\"Up to 100 users\",\"500,000 API calls/month\",\"24/7 phone & chat support\",\"100GB storage\",\"Real-time analytics\",\"Advanced webhooks\",\"SSO & SAML\",\"Audit logs\",\"Custom integrations\"]", SortOrder = 3 };
            var enterprisePlan = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Enterprise", Description = "Unlimited scale with dedicated infrastructure and SLA", StripePriceId = "price_1OxQrBCz3m7kX9aL_ent", StripeProductId = "prod_ent_techflow", Amount = 499.00m, Currency = "usd", Interval = "month", TrialDays = 30, Features = "[\"Unlimited users\",\"Unlimited API calls\",\"Dedicated account manager\",\"Unlimited storage\",\"Custom analytics dashboards\",\"SLA guarantee (99.99%)\",\"On-premise deployment option\",\"HIPAA & SOC2 compliance\",\"Priority bug fixes\"]", SortOrder = 4 };
            var annualPlan = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Professional Annual", Description = "Professional plan billed annually — save 20%", StripePriceId = "price_1OxQrBCz3m7kX9aL_pro_yr", StripeProductId = "prod_pro_techflow", Amount = 470.00m, Currency = "usd", Interval = "year", IntervalCount = 1, TrialDays = 14, Features = "[\"Up to 25 users\",\"50,000 API calls/month\",\"Priority support\",\"25GB storage\",\"Advanced analytics\",\"20% annual discount\"]", SortOrder = 5 };
            db.SubscriptionPlans.AddRange(starterPlan, proPlan, businessPlan, enterprisePlan, annualPlan);

            // Tenant 2 plans
            var basicDental = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenant2Id, Name = "Standard Care", Description = "Individual patient billing plan", StripePriceId = "price_sdn_standard", StripeProductId = "prod_sdn_standard", Amount = 29.00m, Currency = "usd", Interval = "month", Features = "[\"Basic cleanings\",\"X-rays included\",\"10% off procedures\"]", SortOrder = 1 };
            var premiumDental = new Core.Infrastructure.SubscriptionPlan { Id = Guid.NewGuid(), TenantId = tenant2Id, Name = "Premium Care", Description = "Comprehensive family dental plan", StripePriceId = "price_sdn_premium", StripeProductId = "prod_sdn_premium", Amount = 79.00m, Currency = "usd", Interval = "month", Features = "[\"All cleanings & checkups\",\"Full X-rays\",\"25% off procedures\",\"Emergency visits\",\"Family coverage up to 4\"]", SortOrder = 2 };
            db.SubscriptionPlans.AddRange(basicDental, premiumDental);
            db.SaveChanges();
            Console.WriteLine("  [Seed] API Keys & Subscription Plans created.");

            // ─── Customers (Tenant 1 — TechFlow) ──────────────────────
            var cust1 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_R8kLmN3pQ7vX2wY", ExternalReferenceId = "ACC-1001", Email = "david.park@nexacloud.io", Name = "NexaCloud Inc.", Phone = "+1-415-555-0142", Currency = "usd", BillingAddress = "{\"line1\":\"550 Market Street\",\"line2\":\"Suite 1200\",\"city\":\"San Francisco\",\"state\":\"CA\",\"postal_code\":\"94104\",\"country\":\"US\"}", TaxId = "tax_1Ox_nexa_EIN", Metadata = "{\"industry\":\"Cloud Infrastructure\",\"employees\":\"50-200\",\"signupSource\":\"referral\"}" };
            var cust2 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_T4jH8nK2mP5rW9s", ExternalReferenceId = "ACC-1002", Email = "jennifer.lee@brightpath.co", Name = "BrightPath Learning", Phone = "+1-512-555-0298", Currency = "usd", BillingAddress = "{\"line1\":\"200 Congress Avenue\",\"line2\":\"Floor 14\",\"city\":\"Austin\",\"state\":\"TX\",\"postal_code\":\"78701\",\"country\":\"US\"}", TaxId = "tax_1Ox_bp_EIN", Metadata = "{\"industry\":\"EdTech\",\"employees\":\"10-50\",\"signupSource\":\"google_ads\"}" };
            var cust3 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_W2xF6bD9gN1tK8v", ExternalReferenceId = "ACC-1003", Email = "marco.rossi@eurofinance.eu", Name = "EuroFinance GmbH", Phone = "+49-30-555-0187", Currency = "eur", BillingAddress = "{\"line1\":\"Friedrichstraße 123\",\"city\":\"Berlin\",\"state\":\"Berlin\",\"postal_code\":\"10117\",\"country\":\"DE\"}", TaxId = "DE123456789", Metadata = "{\"industry\":\"FinTech\",\"employees\":\"200-500\",\"signupSource\":\"conference\"}" };
            var cust4 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_A9mP3kR7wL5xN2d", ExternalReferenceId = "ACC-1004", Email = "priya.sharma@healthpulse.in", Name = "HealthPulse Technologies", Phone = "+91-80-555-0345", Currency = "usd", BillingAddress = "{\"line1\":\"100 Whitefield Road\",\"city\":\"Bangalore\",\"state\":\"Karnataka\",\"postal_code\":\"560066\",\"country\":\"IN\"}", TaxId = "GSTIN29ABCDE1234F1Z5", Metadata = "{\"industry\":\"HealthTech\",\"employees\":\"50-200\",\"signupSource\":\"partner\"}" };
            var cust5 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_B7nQ4sT8vK6yM1f", ExternalReferenceId = "ACC-1005", Email = "oliver.brown@greenleaf.com.au", Name = "GreenLeaf Analytics", Phone = "+61-2-5550-0412", Currency = "aud", BillingAddress = "{\"line1\":\"42 Pitt Street\",\"city\":\"Sydney\",\"state\":\"NSW\",\"postal_code\":\"2000\",\"country\":\"AU\"}", TaxId = "ABN12345678901", Metadata = "{\"industry\":\"Data Analytics\",\"employees\":\"10-50\",\"signupSource\":\"organic\"}" };
            var cust6 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_C1hR5uV9xJ3zP8g", ExternalReferenceId = "ACC-1006", Email = "sofia.martinez@velocitycrm.mx", Name = "Velocity CRM", Phone = "+52-55-5550-0678", Currency = "usd", BillingAddress = "{\"line1\":\"Av. Reforma 505\",\"line2\":\"Piso 28\",\"city\":\"Mexico City\",\"state\":\"CDMX\",\"postal_code\":\"06600\",\"country\":\"MX\"}", Metadata = "{\"industry\":\"SaaS\",\"employees\":\"10-50\",\"signupSource\":\"product_hunt\"}" };
            var cust7 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_D3iS6wW0yL4aQ9h", ExternalReferenceId = "ACC-1007", Email = "james.wilson@autofleet.co.uk", Name = "AutoFleet Solutions", Phone = "+44-20-5550-0934", Currency = "gbp", BillingAddress = "{\"line1\":\"10 Downing Business Park\",\"city\":\"London\",\"state\":\"England\",\"postal_code\":\"SW1A 2AA\",\"country\":\"GB\"}", TaxId = "GB987654321", Metadata = "{\"industry\":\"Logistics\",\"employees\":\"200-500\",\"signupSource\":\"linkedin_ads\"}" };
            var cust8 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_E5jT7xX1zM5bR0i", ExternalReferenceId = "ACC-1008", Email = "yuki.tanaka@cloudmatrix.jp", Name = "CloudMatrix Japan", Phone = "+81-3-5550-1234", Currency = "jpy", BillingAddress = "{\"line1\":\"2-1-1 Marunouchi\",\"city\":\"Tokyo\",\"state\":\"Tokyo\",\"postal_code\":\"100-0005\",\"country\":\"JP\"}", Metadata = "{\"industry\":\"Cloud Computing\",\"employees\":\"500+\",\"signupSource\":\"sales_outbound\"}" };
            var cust9 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_F2kU8yY2aN6cS1j", ExternalReferenceId = "ACC-1009", Email = "anna.kowalski@devstack.pl", Name = "DevStack Studio", Phone = "+48-22-555-0567", Currency = "eur", BillingAddress = "{\"line1\":\"ul. Marszałkowska 84\",\"city\":\"Warsaw\",\"postal_code\":\"00-514\",\"country\":\"PL\"}", Metadata = "{\"industry\":\"Software Development\",\"employees\":\"10-50\",\"signupSource\":\"twitter\"}" };
            var cust10 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_G4lV9zZ3bO7dT2k", ExternalReferenceId = "ACC-1010", Email = "michael.oconnor@datavault.ie", Name = "DataVault Ireland", Phone = "+353-1-555-0890", Currency = "eur", BillingAddress = "{\"line1\":\"Grand Canal Dock\",\"line2\":\"Block 4\",\"city\":\"Dublin\",\"postal_code\":\"D02 P652\",\"country\":\"IE\"}", TaxId = "IE1234567T", Metadata = "{\"industry\":\"Data Storage\",\"employees\":\"50-200\",\"signupSource\":\"referral\"}" };
            var cust11 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_H6mW0aA4cP8eU3l", ExternalReferenceId = "ACC-1011", Email = "fatima.hassan@codebridge.ae", Name = "CodeBridge DMCC", Phone = "+971-4-555-0123", Currency = "usd", BillingAddress = "{\"line1\":\"DMCC Free Zone\",\"line2\":\"Tower A, Office 1204\",\"city\":\"Dubai\",\"postal_code\":\"00000\",\"country\":\"AE\"}", Metadata = "{\"industry\":\"IT Consulting\",\"employees\":\"50-200\",\"signupSource\":\"event\"}" };
            var cust12 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenantId, StripeCustomerId = "cus_I8nX1bB5dQ9fV4m", ExternalReferenceId = "ACC-1012", Email = "lucas.silva@apimasters.com.br", Name = "API Masters Ltda", Phone = "+55-11-5550-0456", Currency = "usd", BillingAddress = "{\"line1\":\"Av. Paulista 1578\",\"line2\":\"Andar 12\",\"city\":\"São Paulo\",\"state\":\"SP\",\"postal_code\":\"01310-200\",\"country\":\"BR\"}", TaxId = "CNPJ12345678000190", Metadata = "{\"industry\":\"API Development\",\"employees\":\"10-50\",\"signupSource\":\"product_hunt\"}" };
            db.Customers.AddRange(cust1, cust2, cust3, cust4, cust5, cust6, cust7, cust8, cust9, cust10, cust11, cust12);

            // Tenant 2 customers
            var dcust1 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenant2Id, StripeCustomerId = "cus_SDN_01", Email = "john.smith@gmail.com", Name = "John Smith", Phone = "+1-312-555-0101", Currency = "usd", BillingAddress = "{\"line1\":\"742 Evergreen Terrace\",\"city\":\"Chicago\",\"state\":\"IL\",\"postal_code\":\"60614\",\"country\":\"US\"}" };
            var dcust2 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenant2Id, StripeCustomerId = "cus_SDN_02", Email = "maria.garcia@outlook.com", Name = "Maria Garcia", Phone = "+1-312-555-0202", Currency = "usd", BillingAddress = "{\"line1\":\"1600 Oak Avenue\",\"city\":\"Evanston\",\"state\":\"IL\",\"postal_code\":\"60201\",\"country\":\"US\"}" };
            var dcust3 = new Core.Infrastructure.Customer { Id = Guid.NewGuid(), TenantId = tenant2Id, StripeCustomerId = "cus_SDN_03", Email = "robert.johnson@yahoo.com", Name = "Robert Johnson", Phone = "+1-312-555-0303", Currency = "usd", BillingAddress = "{\"line1\":\"350 N LaSalle Drive\",\"city\":\"Chicago\",\"state\":\"IL\",\"postal_code\":\"60654\",\"country\":\"US\"}" };
            db.Customers.AddRange(dcust1, dcust2, dcust3);

            // ─── Subscriptions ─────────────────────────────────────────
            var now = DateTime.UtcNow;
            var sub1 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust1.Id, PlanId = enterprisePlan.Id, StripeSubscriptionId = "sub_1OxEntNexaCloud", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-15), CurrentPeriodEnd = now.AddDays(15), Metadata = "{}", CreatedAt = now.AddMonths(-8) };
            var sub2 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust2.Id, PlanId = proPlan.Id, StripeSubscriptionId = "sub_1OxProBrightPath", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-10), CurrentPeriodEnd = now.AddDays(20), Metadata = "{}", CreatedAt = now.AddMonths(-6) };
            var sub3 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust3.Id, PlanId = businessPlan.Id, StripeSubscriptionId = "sub_1OxBizEuroFin", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-5), CurrentPeriodEnd = now.AddDays(25), Metadata = "{}", CreatedAt = now.AddMonths(-11) };
            var sub4 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust4.Id, PlanId = proPlan.Id, StripeSubscriptionId = "sub_1OxProHealthPulse", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-20), CurrentPeriodEnd = now.AddDays(10), TrialStart = now.AddMonths(-5).AddDays(-14), TrialEnd = now.AddMonths(-5), Metadata = "{}", CreatedAt = now.AddMonths(-5) };
            var sub5 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust5.Id, PlanId = starterPlan.Id, StripeSubscriptionId = "sub_1OxStrGreenLeaf", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-8), CurrentPeriodEnd = now.AddDays(22), Metadata = "{}", CreatedAt = now.AddMonths(-3) };
            var sub6 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust6.Id, PlanId = proPlan.Id, StripeSubscriptionId = "sub_1OxProVelocity", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-12), CurrentPeriodEnd = now.AddDays(18), Metadata = "{}", CreatedAt = now.AddMonths(-4) };
            var sub7 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust7.Id, PlanId = businessPlan.Id, StripeSubscriptionId = "sub_1OxBizAutoFleet", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-3), CurrentPeriodEnd = now.AddDays(27), Metadata = "{}", CreatedAt = now.AddMonths(-7) };
            var sub8 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust8.Id, PlanId = enterprisePlan.Id, StripeSubscriptionId = "sub_1OxEntCloudMatrix", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-1), CurrentPeriodEnd = now.AddDays(29), Metadata = "{}", CreatedAt = now.AddMonths(-10) };
            var sub9 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust9.Id, PlanId = starterPlan.Id, StripeSubscriptionId = "sub_1OxStrDevStack", Status = "trialing", Quantity = 1, CurrentPeriodStart = now.AddDays(-5), CurrentPeriodEnd = now.AddDays(25), TrialStart = now.AddDays(-5), TrialEnd = now.AddDays(9), Metadata = "{}", CreatedAt = now.AddDays(-5) };
            var sub10 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust10.Id, PlanId = businessPlan.Id, StripeSubscriptionId = "sub_1OxBizDataVault", Status = "past_due", Quantity = 1, CurrentPeriodStart = now.AddDays(-35), CurrentPeriodEnd = now.AddDays(-5), Metadata = "{}", CreatedAt = now.AddMonths(-9) };
            var subCancelled = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust11.Id, PlanId = proPlan.Id, StripeSubscriptionId = "sub_1OxProCodeBridge", Status = "canceled", Quantity = 1, CurrentPeriodStart = now.AddDays(-45), CurrentPeriodEnd = now.AddDays(-15), CancelAtPeriodEnd = true, CancelledAt = now.AddDays(-20), CancellationReason = "Switching to annual billing", Metadata = "{}", CreatedAt = now.AddMonths(-6) };
            var sub12 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust12.Id, PlanId = annualPlan.Id, StripeSubscriptionId = "sub_1OxAnnAPIMasters", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddMonths(-2), CurrentPeriodEnd = now.AddMonths(10), Metadata = "{}", CreatedAt = now.AddMonths(-2) };
            db.Subscriptions.AddRange(sub1, sub2, sub3, sub4, sub5, sub6, sub7, sub8, sub9, sub10, subCancelled, sub12);

            // Tenant 2 subscriptions
            var dsub1 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenant2Id, CustomerId = dcust1.Id, PlanId = premiumDental.Id, StripeSubscriptionId = "sub_sdn_01", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-10), CurrentPeriodEnd = now.AddDays(20), Metadata = "{}", CreatedAt = now.AddMonths(-4) };
            var dsub2 = new Core.Infrastructure.Subscription { Id = Guid.NewGuid(), TenantId = tenant2Id, CustomerId = dcust2.Id, PlanId = basicDental.Id, StripeSubscriptionId = "sub_sdn_02", Status = "active", Quantity = 1, CurrentPeriodStart = now.AddDays(-5), CurrentPeriodEnd = now.AddDays(25), Metadata = "{}", CreatedAt = now.AddMonths(-2) };
            db.Subscriptions.AddRange(dsub1, dsub2);
            db.SaveChanges();
            Console.WriteLine("  [Seed] Customers & Subscriptions created.");

            // ─── Payment Transactions ──────────────────────────────────
            string[] brands = { "visa", "mastercard", "amex" };
            string[] last4s = { "4242", "5555", "3782", "1234", "9876" };
            var payments = new List<Core.Infrastructure.PaymentTransaction>();
            var customers = new[] { cust1, cust2, cust3, cust4, cust5, cust6, cust7, cust8, cust9, cust10, cust11, cust12 };
            var subs = new[] { sub1, sub2, sub3, sub4, sub5, sub6, sub7, sub8, sub9, sub10, subCancelled, sub12 };
            decimal[] amounts = { 499.00m, 49.00m, 149.00m, 49.00m, 9.00m, 49.00m, 149.00m, 499.00m, 9.00m, 149.00m, 49.00m, 470.00m };
            var rng = new Random(42);

            for (int month = 5; month >= 0; month--)
            {
                for (int c = 0; c < customers.Length; c++)
                {
                    if (c == 10 && month < 2) continue; // cancelled sub
                    payments.Add(new Core.Infrastructure.PaymentTransaction
                    {
                        Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = customers[c].Id, SubscriptionId = subs[c].Id,
                        StripePaymentIntentId = $"pi_3Ox{month}_{c}_{Guid.NewGuid().ToString("N")[..8]}",
                        StripeChargeId = $"ch_3Ox{month}_{c}_{Guid.NewGuid().ToString("N")[..8]}",
                        StripeCheckoutSessionId = "", Amount = amounts[c], AmountRefunded = 0,
                        Currency = customers[c].Currency, Status = "succeeded", Type = "recurring",
                        PaymentMethod = "card", PaymentMethodBrand = brands[c % 3], PaymentMethodLast4 = last4s[c % 5],
                        Description = $"Subscription payment for {customers[c].Name}",
                        FailureReason = "", ReceiptUrl = $"https://pay.stripe.com/receipts/{Guid.NewGuid().ToString("N")[..16]}",
                        Metadata = "{}", CreatedAt = now.AddMonths(-month).AddDays(rng.Next(-5, 5))
                    });
                }
            }
            // One-time payments
            payments.Add(new Core.Infrastructure.PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust1.Id, StripePaymentIntentId = "pi_3Ox_ot_nexa_setup", StripeChargeId = "ch_3Ox_ot_nexa_setup", StripeCheckoutSessionId = "cs_3Ox_ot_nexa_setup", Amount = 2500.00m, Currency = "usd", Status = "succeeded", Type = "one_time", PaymentMethod = "card", PaymentMethodBrand = "visa", PaymentMethodLast4 = "4242", Description = "Enterprise setup and onboarding fee", FailureReason = "", ReceiptUrl = "", Metadata = "{\"type\":\"setup_fee\"}", CreatedAt = now.AddMonths(-8) });
            payments.Add(new Core.Infrastructure.PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust8.Id, StripePaymentIntentId = "pi_3Ox_ot_cloud_addon", StripeChargeId = "ch_3Ox_ot_cloud_addon", StripeCheckoutSessionId = "", Amount = 1200.00m, Currency = "jpy", Status = "succeeded", Type = "one_time", PaymentMethod = "card", PaymentMethodBrand = "mastercard", PaymentMethodLast4 = "5555", Description = "Additional storage add-on (500GB)", FailureReason = "", ReceiptUrl = "", Metadata = "{\"type\":\"addon\"}", CreatedAt = now.AddMonths(-3) });
            // Failed payment
            var failedPmt = new Core.Infrastructure.PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust10.Id, StripePaymentIntentId = "pi_3Ox_fail_dv", StripeChargeId = "", StripeCheckoutSessionId = "", Amount = 149.00m, Currency = "eur", Status = "failed", Type = "recurring", PaymentMethod = "card", PaymentMethodBrand = "visa", PaymentMethodLast4 = "9876", Description = "Failed subscription renewal", FailureReason = "card_declined: insufficient_funds", ReceiptUrl = "", Metadata = "{}", CreatedAt = now.AddDays(-5) };
            payments.Add(failedPmt);
            db.PaymentTransactions.AddRange(payments);

            // Tenant 2 payments
            for (int m = 3; m >= 0; m--)
            {
                db.PaymentTransactions.Add(new Core.Infrastructure.PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenant2Id, CustomerId = dcust1.Id, SubscriptionId = dsub1.Id, StripePaymentIntentId = $"pi_sdn_01_{m}", StripeChargeId = $"ch_sdn_01_{m}", StripeCheckoutSessionId = "", Amount = 79.00m, Currency = "usd", Status = "succeeded", Type = "recurring", PaymentMethod = "card", PaymentMethodBrand = "visa", PaymentMethodLast4 = "6789", Description = "Premium Care monthly", FailureReason = "", ReceiptUrl = "", Metadata = "{}", CreatedAt = now.AddMonths(-m) });
                db.PaymentTransactions.Add(new Core.Infrastructure.PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenant2Id, CustomerId = dcust2.Id, SubscriptionId = dsub2.Id, StripePaymentIntentId = $"pi_sdn_02_{m}", StripeChargeId = $"ch_sdn_02_{m}", StripeCheckoutSessionId = "", Amount = 29.00m, Currency = "usd", Status = "succeeded", Type = "recurring", PaymentMethod = "card", PaymentMethodBrand = "mastercard", PaymentMethodLast4 = "2345", Description = "Standard Care monthly", FailureReason = "", ReceiptUrl = "", Metadata = "{}", CreatedAt = now.AddMonths(-m) });
            }

            // ─── Invoices ──────────────────────────────────────────────
            var invoices = new List<Core.Infrastructure.Invoice>();
            int invoiceNum = 1001;
            for (int month = 5; month >= 0; month--)
            {
                for (int c = 0; c < customers.Length; c++)
                {
                    if (c == 10 && month < 2) continue;
                    invoices.Add(new Core.Infrastructure.Invoice
                    {
                        Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = customers[c].Id, SubscriptionId = subs[c].Id,
                        StripeInvoiceId = $"in_3Ox{month}_{c}_{invoiceNum}", InvoiceNumber = $"TFL-{invoiceNum++}",
                        Subtotal = amounts[c], Tax = Math.Round(amounts[c] * 0.08m, 2),
                        Total = amounts[c] + Math.Round(amounts[c] * 0.08m, 2),
                        AmountPaid = amounts[c] + Math.Round(amounts[c] * 0.08m, 2), AmountDue = 0,
                        Currency = customers[c].Currency, Status = "paid", InvoicePdfUrl = "", HostedInvoiceUrl = "",
                        LineItems = $"[{{\"description\":\"Subscription\",\"amount\":{amounts[c]},\"quantity\":1}}]",
                        PaidAt = now.AddMonths(-month).AddDays(1), DueDate = now.AddMonths(-month).AddDays(15),
                        PeriodStart = now.AddMonths(-month), PeriodEnd = now.AddMonths(-month + 1),
                        CreatedAt = now.AddMonths(-month)
                    });
                }
            }
            // Unpaid invoice
            invoices.Add(new Core.Infrastructure.Invoice
            {
                Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = cust10.Id, SubscriptionId = sub10.Id,
                StripeInvoiceId = "in_3Ox_unpaid_dv", InvoiceNumber = $"TFL-{invoiceNum++}",
                Subtotal = 149.00m, Tax = 11.92m, Total = 160.92m, AmountPaid = 0, AmountDue = 160.92m,
                Currency = "eur", Status = "open", InvoicePdfUrl = "", HostedInvoiceUrl = "",
                LineItems = "[{\"description\":\"Business Plan\",\"amount\":149.00,\"quantity\":1}]",
                DueDate = now.AddDays(10), PeriodStart = now.AddDays(-5), PeriodEnd = now.AddDays(25), CreatedAt = now.AddDays(-5)
            });
            db.Invoices.AddRange(invoices);
            db.SaveChanges();
            Console.WriteLine("  [Seed] Payments & Invoices created.");

            // ─── Refunds ───────────────────────────────────────────────
            var refundPmt1 = payments.First(p => p.CustomerId == cust2.Id && p.Status == "succeeded");
            db.Refunds.Add(new Core.Infrastructure.Refund { Id = Guid.NewGuid(), TenantId = tenantId, TransactionId = refundPmt1.Id, CustomerId = cust2.Id, StripeRefundId = "re_3Ox_bp_partial", Amount = 24.50m, Currency = "usd", Reason = "duplicate", Notes = "Customer was double-charged due to retry logic", Status = "succeeded", ApprovedBy = adminUser.Email, ApprovedAt = now.AddDays(-18), ProcessedAt = now.AddDays(-18), CreatedAt = now.AddDays(-19) });
            var refundPmt2 = payments.First(p => p.CustomerId == cust6.Id && p.Status == "succeeded");
            db.Refunds.Add(new Core.Infrastructure.Refund { Id = Guid.NewGuid(), TenantId = tenantId, TransactionId = refundPmt2.Id, CustomerId = cust6.Id, StripeRefundId = "re_3Ox_vel_full", Amount = 49.00m, Currency = "usd", Reason = "requested_by_customer", Notes = "Service downtime during billing period", Status = "succeeded", ApprovedBy = superAdminUser.Email, ApprovedAt = now.AddDays(-10), ProcessedAt = now.AddDays(-10), CreatedAt = now.AddDays(-11) });
            db.Refunds.Add(new Core.Infrastructure.Refund { Id = Guid.NewGuid(), TenantId = tenantId, TransactionId = payments.First(p => p.CustomerId == cust7.Id && p.Status == "succeeded").Id, CustomerId = cust7.Id, StripeRefundId = "", Amount = 50.00m, Currency = "gbp", Reason = "requested_by_customer", Notes = "Partial credit for unused period after plan downgrade", Status = "pending", ApprovedBy = "", CreatedAt = now.AddDays(-2) });

            // ─── Coupons & Promotion Codes ─────────────────────────────
            var coupon1 = new Core.Infrastructure.Coupon { Id = Guid.NewGuid(), TenantId = tenantId, StripeCouponId = "cpn_welcome20", Name = "Welcome 20% Off", Type = "percent_off", PercentOff = 20.0m, Duration = "once", MaxRedemptions = 500, TimesRedeemed = 147, IsActive = true, RedeemBy = now.AddMonths(6), Metadata = "{\"campaign\":\"new_signups_2026\"}" };
            var coupon2 = new Core.Infrastructure.Coupon { Id = Guid.NewGuid(), TenantId = tenantId, StripeCouponId = "cpn_annual50", Name = "Annual Plan $50 Off", Type = "amount_off", AmountOff = 50.00m, Currency = "usd", Duration = "once", MaxRedemptions = 200, TimesRedeemed = 34, IsActive = true, RedeemBy = now.AddMonths(3), Metadata = "{\"campaign\":\"annual_promo_q1\"}" };
            var coupon3 = new Core.Infrastructure.Coupon { Id = Guid.NewGuid(), TenantId = tenantId, StripeCouponId = "cpn_partner15", Name = "Partner 15% Recurring", Type = "percent_off", PercentOff = 15.0m, Duration = "repeating", DurationInMonths = 6, MaxRedemptions = 100, TimesRedeemed = 12, IsActive = true, Metadata = "{\"campaign\":\"partner_program\"}" };
            var coupon4 = new Core.Infrastructure.Coupon { Id = Guid.NewGuid(), TenantId = tenantId, StripeCouponId = "cpn_blackfriday", Name = "Black Friday 30% Off", Type = "percent_off", PercentOff = 30.0m, Duration = "once", MaxRedemptions = 1000, TimesRedeemed = 823, IsActive = false, RedeemBy = now.AddMonths(-3), Metadata = "{\"campaign\":\"black_friday_2025\"}" };
            db.Coupons.AddRange(coupon1, coupon2, coupon3, coupon4);

            db.PromotionCodes.Add(new Core.Infrastructure.PromotionCode { TenantId = tenantId, CouponId = coupon1.Id, Code = "WELCOME20", MaxRedemptions = 500, TimesRedeemed = 147, IsActive = true, ExpiresAt = now.AddMonths(6), Restrictions = "{\"firstTimeTransaction\":true}" });
            db.PromotionCodes.Add(new Core.Infrastructure.PromotionCode { TenantId = tenantId, CouponId = coupon2.Id, Code = "ANNUAL50", MaxRedemptions = 200, TimesRedeemed = 34, IsActive = true, ExpiresAt = now.AddMonths(3), Restrictions = "{\"minAmount\":470}" });
            db.PromotionCodes.Add(new Core.Infrastructure.PromotionCode { TenantId = tenantId, CouponId = coupon3.Id, Code = "PARTNER15", MaxRedemptions = 100, TimesRedeemed = 12, IsActive = true, Restrictions = "{}" });
            db.PromotionCodes.Add(new Core.Infrastructure.PromotionCode { TenantId = tenantId, CouponId = coupon4.Id, Code = "BFRIDAY30", MaxRedemptions = 1000, TimesRedeemed = 823, IsActive = false, ExpiresAt = now.AddMonths(-3), Restrictions = "{}" });

            db.CouponRedemptions.Add(new Core.Infrastructure.CouponRedemption { TenantId = tenantId, CouponId = coupon1.Id, CustomerId = cust5.Id, SubscriptionId = sub5.Id, AmountDiscounted = 1.80m, Currency = "aud" });
            db.CouponRedemptions.Add(new Core.Infrastructure.CouponRedemption { TenantId = tenantId, CouponId = coupon2.Id, CustomerId = cust12.Id, SubscriptionId = sub12.Id, AmountDiscounted = 50.00m, Currency = "usd" });
            db.CouponRedemptions.Add(new Core.Infrastructure.CouponRedemption { TenantId = tenantId, CouponId = coupon3.Id, CustomerId = cust4.Id, SubscriptionId = sub4.Id, AmountDiscounted = 7.35m, Currency = "usd" });

            // ─── Webhook Subscriptions & Deliveries ─────────────────────
            var wh1 = new Core.Infrastructure.WebhookSubscription { Id = Guid.NewGuid(), TenantId = tenantId, WebhookUrl = "https://api.nexacloud.io/billing/webhooks", TargetUrl = "https://api.nexacloud.io/billing/webhooks", HmacSecret = "whsec_nexa_hmac_secret_key_2026", Secret = "whsec_nexa_hmac_secret_key_2026", Events = "[\"payment.succeeded\",\"payment.failed\",\"subscription.created\",\"subscription.updated\",\"subscription.canceled\",\"invoice.paid\"]", CustomHeaders = "{\"X-Source\":\"TechFlow\"}", RetryPolicy = "exponential_backoff", MaxRetries = 5, Timeout = 30, Description = "NexaCloud production webhook", Metadata = "{}", IsActive = true };
            var wh2 = new Core.Infrastructure.WebhookSubscription { Id = Guid.NewGuid(), TenantId = tenantId, WebhookUrl = "https://hooks.slack.com/services/T0ABC/B0DEF/xYzAbCdEfG", TargetUrl = "https://hooks.slack.com/services/T0ABC/B0DEF/xYzAbCdEfG", HmacSecret = "whsec_slack_notify_key", Secret = "whsec_slack_notify_key", Events = "[\"payment.failed\",\"subscription.canceled\",\"refund.created\"]", CustomHeaders = "{}", RetryPolicy = "fixed", MaxRetries = 3, Timeout = 10, Description = "Slack alerts for critical billing events", Metadata = "{}", IsActive = true };
            var wh3 = new Core.Infrastructure.WebhookSubscription { Id = Guid.NewGuid(), TenantId = tenantId, WebhookUrl = "https://analytics.techflow.io/events", TargetUrl = "https://analytics.techflow.io/events", HmacSecret = "whsec_analytics_key_2026", Secret = "whsec_analytics_key_2026", Events = "[\"payment.succeeded\",\"subscription.created\",\"customer.created\"]", CustomHeaders = "{\"X-Analytics-Source\":\"billing\"}", RetryPolicy = "exponential_backoff", MaxRetries = 5, Timeout = 15, Description = "Internal analytics pipeline", Metadata = "{}", IsActive = true };
            db.WebhookSubscriptions.AddRange(wh1, wh2, wh3);

            db.WebhookDeliveries.Add(new Core.Infrastructure.WebhookDelivery { TenantId = tenantId, WebhookSubscriptionId = wh1.Id, EventType = "payment.succeeded", EventData = "{}", TargetUrl = wh1.TargetUrl, Payload = "{}", Status = "Delivered", HttpStatusCode = 200, StatusCode = 200, ResponseBody = "{\"received\":true}", DurationMs = 245, RetryCount = 0, MaxAttempts = 5, MaxRetries = 5, FailureReason = "", LastError = "", DeliveredAt = now.AddHours(-2), CreatedAt = now.AddHours(-2) });
            db.WebhookDeliveries.Add(new Core.Infrastructure.WebhookDelivery { TenantId = tenantId, WebhookSubscriptionId = wh2.Id, EventType = "payment.failed", EventData = "{}", TargetUrl = wh2.TargetUrl, Payload = "{}", Status = "Delivered", HttpStatusCode = 200, StatusCode = 200, ResponseBody = "ok", DurationMs = 312, RetryCount = 1, MaxAttempts = 3, MaxRetries = 3, FailureReason = "", LastError = "Connection timeout on first attempt", DeliveredAt = now.AddDays(-5), CreatedAt = now.AddDays(-5) });
            db.WebhookDeliveries.Add(new Core.Infrastructure.WebhookDelivery { TenantId = tenantId, WebhookSubscriptionId = wh3.Id, EventType = "customer.created", EventData = "{}", TargetUrl = wh3.TargetUrl, Payload = "{}", Status = "Failed", HttpStatusCode = 502, StatusCode = 502, ResponseBody = "Bad Gateway", DurationMs = 5000, RetryCount = 5, MaxAttempts = 5, MaxRetries = 5, FailureReason = "Max retries exceeded", LastError = "502 Bad Gateway", FailedAt = now.AddDays(-3), CreatedAt = now.AddDays(-3) });

            // ─── Inbound Webhook Events ─────────────────────────────────
            db.WebhookEventsInbound.Add(new Core.Infrastructure.WebhookEventInbound { TenantId = tenantId, StripeEventId = "evt_3Ox_pi_succeeded_01", EventType = "payment_intent.succeeded", Data = "{}", Payload = "{}", Status = "processed", ProcessingError = "", ProcessedAt = now.AddHours(-2), ReceivedAt = now.AddHours(-2) });
            db.WebhookEventsInbound.Add(new Core.Infrastructure.WebhookEventInbound { TenantId = tenantId, StripeEventId = "evt_3Ox_inv_paid_01", EventType = "invoice.paid", Data = "{}", Payload = "{}", Status = "processed", ProcessingError = "", ProcessedAt = now.AddHours(-4), ReceivedAt = now.AddHours(-4) });
            db.WebhookEventsInbound.Add(new Core.Infrastructure.WebhookEventInbound { TenantId = tenantId, StripeEventId = "evt_3Ox_ch_failed_01", EventType = "charge.failed", Data = "{}", Payload = "{}", Status = "failed", ProcessingError = "Handler threw exception: Customer not found", RetryCount = 2, ReceivedAt = now.AddDays(-5) });

            // ─── Dunning ────────────────────────────────────────────────
            db.DunningSchedules.Add(new Core.Infrastructure.DunningSchedule { TenantId = tenantId, SubscriptionId = sub10.Id, CustomerId = cust10.Id, StripeInvoiceId = "in_3Ox_unpaid_dv", Status = "active", CurrentStep = 2, MaxSteps = 4, NextRetryAt = now.AddDays(2), LastRetryAt = now.AddDays(-3), TotalRetryAttempts = 2, OriginalFailureDate = now.AddDays(-5), FailureReason = "card_declined: insufficient_funds", AmountDue = 160.92m, Currency = "eur", GracePeriodEndsAt = now.AddDays(10) });
            db.DunningSteps.Add(new Core.Infrastructure.DunningStep { TenantId = tenantId, SortOrder = 1, DaysAfterFailure = 1, Action = "retry_payment", EmailTemplateKey = "payment_retry_1", IsActive = true });
            db.DunningSteps.Add(new Core.Infrastructure.DunningStep { TenantId = tenantId, SortOrder = 2, DaysAfterFailure = 3, Action = "send_email", EmailTemplateKey = "payment_failed_notice", IsActive = true });
            db.DunningSteps.Add(new Core.Infrastructure.DunningStep { TenantId = tenantId, SortOrder = 3, DaysAfterFailure = 7, Action = "retry_payment", EmailTemplateKey = "payment_retry_final", IsActive = true });
            db.DunningSteps.Add(new Core.Infrastructure.DunningStep { TenantId = tenantId, SortOrder = 4, DaysAfterFailure = 14, Action = "cancel_subscription", EmailTemplateKey = "subscription_cancelled", IsActive = true });

            // ─── Tax ────────────────────────────────────────────────────
            db.TaxConfigurations.Add(new Core.Infrastructure.TaxConfiguration { TenantId = tenantId, Provider = "stripe_tax", IsEnabled = true, AutoCalculate = true, DefaultTaxBehavior = "exclusive", FallbackTaxRate = 0.0800m, RegistrationNumbers = "[{\"country\":\"US\",\"type\":\"EIN\",\"value\":\"12-3456789\"},{\"country\":\"DE\",\"type\":\"VAT\",\"value\":\"DE123456789\"}]" });
            db.TaxConfigurations.Add(new Core.Infrastructure.TaxConfiguration { TenantId = tenant2Id, Provider = "stripe_tax", IsEnabled = true, AutoCalculate = true, DefaultTaxBehavior = "inclusive", FallbackTaxRate = 0.0625m, RegistrationNumbers = "[{\"country\":\"US\",\"type\":\"EIN\",\"value\":\"98-7654321\"}]" });
            db.TaxExemptions.Add(new Core.Infrastructure.TaxExemption { TenantId = tenantId, CustomerId = cust3.Id, ExemptionType = "reverse", CertificateId = "EU-REV-2025-0042", ValidFrom = now.AddMonths(-11), ValidTo = now.AddMonths(13) });
            db.TaxExemptions.Add(new Core.Infrastructure.TaxExemption { TenantId = tenantId, CustomerId = cust4.Id, ExemptionType = "exempt", CertificateId = "IN-EXM-2025-HP-0017", ValidFrom = now.AddMonths(-5), ValidTo = now.AddMonths(7) });

            // ─── Email Templates & Logs ─────────────────────────────────
            db.EmailTemplates.Add(new Core.Infrastructure.EmailTemplate { TenantId = tenantId, TemplateKey = "welcome", Subject = "Welcome to TechFlow, {{customer_name}}!", HtmlBody = "<h1>Welcome aboard!</h1><p>Hi {{customer_name}}, your account is ready.</p>", Variables = "[\"customer_name\",\"plan_name\",\"login_url\"]" });
            db.EmailTemplates.Add(new Core.Infrastructure.EmailTemplate { TenantId = tenantId, TemplateKey = "invoice_paid", Subject = "Invoice {{invoice_number}} — Payment Received", HtmlBody = "<p>Hi {{customer_name}}, payment of {{amount}} received for {{invoice_number}}.</p>", Variables = "[\"customer_name\",\"invoice_number\",\"amount\",\"date\"]" });
            db.EmailTemplates.Add(new Core.Infrastructure.EmailTemplate { TenantId = tenantId, TemplateKey = "payment_failed_notice", Subject = "Action Required: Payment Failed for {{plan_name}}", HtmlBody = "<p>Hi {{customer_name}}, your payment of {{amount}} failed. Please update your payment method.</p>", Variables = "[\"customer_name\",\"amount\",\"plan_name\",\"update_url\"]" });
            db.EmailTemplates.Add(new Core.Infrastructure.EmailTemplate { TenantId = tenantId, TemplateKey = "payment_retry_1", Subject = "We'll retry your payment soon", HtmlBody = "<p>Hi {{customer_name}}, we'll retry your payment of {{amount}} in 24 hours.</p>", Variables = "[\"customer_name\",\"amount\",\"retry_date\"]" });
            db.EmailTemplates.Add(new Core.Infrastructure.EmailTemplate { TenantId = tenantId, TemplateKey = "payment_retry_final", Subject = "Final payment retry — {{plan_name}}", HtmlBody = "<p>Hi {{customer_name}}, this is our last attempt to process {{amount}}.</p>", Variables = "[\"customer_name\",\"amount\",\"plan_name\",\"update_url\"]" });
            db.EmailTemplates.Add(new Core.Infrastructure.EmailTemplate { TenantId = tenantId, TemplateKey = "subscription_cancelled", Subject = "Your {{plan_name}} subscription has been cancelled", HtmlBody = "<p>Hi {{customer_name}}, your {{plan_name}} subscription was cancelled due to non-payment.</p>", Variables = "[\"customer_name\",\"plan_name\",\"reactivate_url\"]" });

            db.EmailLogs.Add(new Core.Infrastructure.EmailLog { TenantId = tenantId, TemplateKey = "invoice_paid", To = "david.park@nexacloud.io", Subject = "Invoice TFL-1001 — Payment Received", Status = "delivered", Provider = "sendgrid", ProviderMessageId = "sg_msg_001", SentAt = now.AddDays(-15), DeliveredAt = now.AddDays(-15) });
            db.EmailLogs.Add(new Core.Infrastructure.EmailLog { TenantId = tenantId, TemplateKey = "payment_failed_notice", To = "michael.oconnor@datavault.ie", Subject = "Action Required: Payment Failed", Status = "delivered", Provider = "sendgrid", ProviderMessageId = "sg_msg_002", SentAt = now.AddDays(-5), DeliveredAt = now.AddDays(-5) });
            db.EmailLogs.Add(new Core.Infrastructure.EmailLog { TenantId = tenantId, TemplateKey = "welcome", To = "anna.kowalski@devstack.pl", Subject = "Welcome to TechFlow!", Status = "delivered", Provider = "sendgrid", ProviderMessageId = "sg_msg_003", SentAt = now.AddDays(-5), DeliveredAt = now.AddDays(-5) });

            // ─── Customer Credits ───────────────────────────────────────
            db.CustomerCredits.Add(new Core.Infrastructure.CustomerCredit { TenantId = tenantId, CustomerId = cust2.Id, Type = "credit", Amount = 24.50m, Currency = "usd", Description = "Credit from partial refund — duplicate charge", Source = "refund", BalanceAfter = 24.50m, CreatedBy = adminUser.Id });
            db.CustomerCredits.Add(new Core.Infrastructure.CustomerCredit { TenantId = tenantId, CustomerId = cust6.Id, Type = "credit", Amount = 49.00m, Currency = "usd", Description = "Service downtime compensation", Source = "manual", BalanceAfter = 49.00m, CreatedBy = superAdminUser.Id });
            db.CustomerCredits.Add(new Core.Infrastructure.CustomerCredit { TenantId = tenantId, CustomerId = cust1.Id, Type = "credit", Amount = 100.00m, Currency = "usd", Description = "Loyalty reward — 12 months active", Source = "promotion", BalanceAfter = 100.00m, CreatedBy = superAdminUser.Id });
            db.CustomerCredits.Add(new Core.Infrastructure.CustomerCredit { TenantId = tenantId, CustomerId = cust9.Id, Type = "credit", Amount = 9.00m, Currency = "eur", Description = "Trial conversion bonus", Source = "promotion", BalanceAfter = 9.00m, CreatedBy = managerUser.Id });

            // ─── Stripe Connect ─────────────────────────────────────────
            db.ConnectedAccounts.Add(new Core.Infrastructure.ConnectedAccount { TenantId = tenantId, StripeAccountId = "acct_1OxCA_nexacloud", BusinessName = "NexaCloud Infrastructure", Email = "billing@nexacloud.io", Country = "US", Type = "express", ChargesEnabled = true, PayoutsEnabled = true, OnboardingComplete = true, PlatformFeePercent = 2.5m, PlatformFeeFixed = 0.30m, Metadata = "{\"partnerTier\":\"gold\"}" });
            db.ConnectedAccounts.Add(new Core.Infrastructure.ConnectedAccount { TenantId = tenantId, StripeAccountId = "acct_1OxCA_eurofinance", BusinessName = "EuroFinance Payments", Email = "connect@eurofinance.eu", Country = "DE", Type = "standard", ChargesEnabled = true, PayoutsEnabled = true, OnboardingComplete = true, PlatformFeePercent = 1.8m, PlatformFeeFixed = 0.25m, Metadata = "{\"partnerTier\":\"platinum\"}" });
            db.ConnectedAccounts.Add(new Core.Infrastructure.ConnectedAccount { TenantId = tenantId, StripeAccountId = "acct_1OxCA_autofleet", BusinessName = "AutoFleet Pay", Email = "payments@autofleet.co.uk", Country = "GB", Type = "express", ChargesEnabled = true, PayoutsEnabled = false, OnboardingComplete = false, PlatformFeePercent = 3.0m, PlatformFeeFixed = 0.50m, Metadata = "{\"partnerTier\":\"silver\"}" });

            // ─── Usage Records & Meter Events ───────────────────────────
            for (int d = 30; d >= 0; d -= 3)
            {
                db.UsageRecords.Add(new Core.Infrastructure.UsageRecord { TenantId = tenantId, SubscriptionId = sub1.Id, Quantity = rng.Next(800, 2500), Timestamp = now.AddDays(-d), Action = "increment", CreatedAt = now.AddDays(-d) });
                db.UsageRecords.Add(new Core.Infrastructure.UsageRecord { TenantId = tenantId, SubscriptionId = sub8.Id, Quantity = rng.Next(1500, 5000), Timestamp = now.AddDays(-d), Action = "increment", CreatedAt = now.AddDays(-d) });
            }
            db.MeterEvents.Add(new Core.Infrastructure.MeterEvent { TenantId = tenantId, CustomerId = cust1.Id, EventName = "api_calls", Value = 48250, Timestamp = now.AddDays(-1), Properties = "{\"region\":\"us-west-2\"}" });
            db.MeterEvents.Add(new Core.Infrastructure.MeterEvent { TenantId = tenantId, CustomerId = cust8.Id, EventName = "api_calls", Value = 127890, Timestamp = now.AddDays(-1), Properties = "{\"region\":\"ap-northeast-1\"}" });

            // ─── Settings ───────────────────────────────────────────────
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenantId, Key = "invoice_prefix", Value = "TFL", ValueType = "string", Description = "Invoice number prefix" });
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenantId, Key = "default_currency", Value = "usd", ValueType = "string", Description = "Default billing currency" });
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenantId, Key = "payment_retry_days", Value = "7", ValueType = "int", Description = "Days to retry failed payments" });
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenantId, Key = "auto_invoice", Value = "true", ValueType = "bool", Description = "Auto-generate invoices on payment" });
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenantId, Key = "dunning_enabled", Value = "true", ValueType = "bool", Description = "Enable automated dunning" });
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenant2Id, Key = "invoice_prefix", Value = "SDG", ValueType = "string", Description = "Invoice number prefix" });
            db.Settings.Add(new Core.Infrastructure.Setting { TenantId = tenant2Id, Key = "default_currency", Value = "usd", ValueType = "string", Description = "Default billing currency" });

            // ─── Audit Logs ─────────────────────────────────────────────
            db.AuditLogs.Add(new Core.Infrastructure.AuditLog { TenantId = tenantId, UserId = superAdminUser.Id, UserEmail = superAdminUser.Email, Action = "login", EntityType = "User", EntityId = superAdminUser.Id.ToString(), Details = "Successful login from 192.168.1.10", Status = "success", IPAddress = "192.168.1.10", UserAgent = "Mozilla/5.0 Chrome/120.0", CreatedAt = now.AddHours(-2) });
            db.AuditLogs.Add(new Core.Infrastructure.AuditLog { TenantId = tenantId, UserId = adminUser.Id, UserEmail = adminUser.Email, Action = "refund.approved", EntityType = "Refund", EntityId = Guid.NewGuid().ToString(), Details = "Approved partial refund of $24.50 for BrightPath Learning", Status = "success", IPAddress = "10.0.0.45", CreatedAt = now.AddDays(-18) });
            db.AuditLogs.Add(new Core.Infrastructure.AuditLog { TenantId = tenantId, UserId = superAdminUser.Id, UserEmail = superAdminUser.Email, Action = "coupon.created", EntityType = "Coupon", EntityId = coupon1.Id.ToString(), Details = "Created coupon 'Welcome 20% Off'", Status = "success", IPAddress = "192.168.1.10", CreatedAt = now.AddMonths(-6) });
            db.AuditLogs.Add(new Core.Infrastructure.AuditLog { TenantId = tenantId, UserId = adminUser.Id, UserEmail = adminUser.Email, Action = "subscription.canceled", EntityType = "Subscription", EntityId = subCancelled.Id.ToString(), Details = "Cancelled subscription for CodeBridge DMCC", Status = "success", IPAddress = "10.0.0.45", CreatedAt = now.AddDays(-20) });

            // ─── API Call Logs ──────────────────────────────────────────
            db.ApiCallLogs.Add(new Core.Infrastructure.ApiCallLog { TenantId = tenantId, ServiceType = "stripe", Endpoint = "/v1/payment_intents", Method = "POST", RequestBody = "{\"amount\":49900}", ResponseStatusCode = 200, StatusCode = 200, ResponseBody = "{\"id\":\"pi_xxx\"}", DurationMs = 342, ResponseTime = 342, RequestSize = 45, ResponseSize = 890, UserAgent = "stripe-dotnet/47.0.0", Success = true, Status = "success", ErrorMessage = "", IpAddress = "10.0.0.1", CreatedAt = now.AddHours(-2) });
            db.ApiCallLogs.Add(new Core.Infrastructure.ApiCallLog { TenantId = tenantId, ServiceType = "stripe", Endpoint = "/v1/refunds", Method = "POST", RequestBody = "{\"charge\":\"ch_xxx\"}", ResponseStatusCode = 402, StatusCode = 402, ResponseBody = "{\"error\":{\"message\":\"Charge already refunded\"}}", DurationMs = 156, ResponseTime = 156, RequestSize = 32, ResponseSize = 120, UserAgent = "stripe-dotnet/47.0.0", Success = false, Status = "error", ErrorMessage = "Charge already refunded", IpAddress = "10.0.0.1", CreatedAt = now.AddDays(-3) });

            // ─── Rate Limits ────────────────────────────────────────────
            db.EndpointRateLimits.Add(new Core.Infrastructure.EndpointRateLimitConfig { TenantId = tenantId, Endpoint = "POST /api/v1/payments/*", RequestsPerMinute = 30, BurstLimit = 50, IsActive = true });
            db.EndpointRateLimits.Add(new Core.Infrastructure.EndpointRateLimitConfig { TenantId = tenantId, Endpoint = "GET /api/v1/customers/*", RequestsPerMinute = 120, BurstLimit = 200, IsActive = true });
            db.EndpointRateLimits.Add(new Core.Infrastructure.EndpointRateLimitConfig { TenantId = tenantId, Endpoint = "POST /api/v1/webhooks/*", RequestsPerMinute = 10, BurstLimit = 15, IsActive = true });

            db.SaveChanges();
            Console.WriteLine("  [Seed] Remaining entities created.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [Seed] Database seeding completed successfully!");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("  DATABASE SEED FAILED");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine(ex.ToString());
        if (ex.InnerException != null)
        {
            Console.WriteLine("─── Inner Exception ───");
            Console.WriteLine(ex.InnerException.ToString());
        }
        Console.ResetColor();
        Log.Logger.Warning(ex, "Database seed failed - will retry on next startup");
    }
}

app.Run();
