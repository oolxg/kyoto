using Microsoft.EntityFrameworkCore;
using Smug.Middlewares;
using Smug.Models.SmugDbContext;
using Smug.Services.Implementations;
using Smug.Services.Interfaces;

namespace Smug;

public class Startup
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddDbContext<SmugDbContext>(options =>
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
        
        app.UseWhen(context => context.Request.Path == "/api/v1/check", appBuilder =>
        {
            appBuilder.UseMiddleware<RequestSaverMiddleware>();
        });
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDoApp v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}