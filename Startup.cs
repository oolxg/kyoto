using Kyoto.Middlewares;
using Kyoto.Models.KyotoDbContext;
using Kyoto.Services.Implementations;
using Kyoto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Kyoto;

public class Startup
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.

        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
        
        builder.Services.AddDbContext<KyotoDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging();
        });

        builder.Services.AddScoped<IUserRequestRepository, UserRequestRepository>();
        builder.Services.AddScoped<IRestrictedUrlRepository, RestrictedUrlRepository>();
        builder.Services.AddScoped<IIpRepository, IpRepository>();
        builder.Services.AddScoped<ITokenRepository, TokenRepository>();
        builder.Services.AddScoped<IAccessValidator, AccessValidator>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseWhen(context => context.Request.Path == "/api/v1/check",
            appBuilder => appBuilder.UseMiddleware<RequestSaverMiddleware>());

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Kyoto");
                options.RoutePrefix = string.Empty;
            });
        }

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<KyotoDbContext>();
            await context.Database.MigrateAsync();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}