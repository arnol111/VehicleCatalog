using API.Infrastructure;
using Scalar.AspNetCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/openapi/v1.json",
                    "Vehicle API v1"
                );

                options.RoutePrefix = "swagger";
            });
        }

        // HTTP plain is required in Development so local clients that cannot trust
        // the dev certificate (Android emulator via 10.0.2.2) can consume the API.
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}