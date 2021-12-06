// Configure services

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = "https://login.microsoftonline.com/xxxxxxxxxxxxxxxxxxxxxxxxxx";
    options.Audience = "xxxxxxxxxxxxxxxxxxxxxxxxx";
    options.TokenValidationParameters.ValidateLifetime = false;
    options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
});

builder.Services.AddAuthorization();

builder.Services.AddCors();

builder.Services.AddScoped<IHelloService, HelloService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
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
        }});
});

// Configure and enable middlewares
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));

app.UseCors(p =>
{
    p.AllowAnyOrigin();
    p.WithMethods("GET");
    p.AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

// GET endpoint where IHelloService and ClaimsPrincipal and injected by dependency no needs to use anymore [FromServices] attribute
// ClaimsPrincipal is automatically injected
app.MapGet("/Hello", (bool? isHappy, IHelloService service, ClaimsPrincipal user) =>
{
    if (isHappy is null)
        return Results.BadRequest("Please tell if you are happy or not :-)");

    return Results.Ok(service.Hello(user, (bool)isHappy));
}).RequireAuthorization();

// Run the app
app.Run();