using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Siganberg.SerilogElasticSearch.Extensions;

namespace Siganberg.SerilogElasticSearch.SampleApp;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.UseSerilog();
        
        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestApi", Version = "v1" });
        });
        
        var app = builder.Build();
        
        app.UseRequestLogging();
            
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestApi v1"));
        }
            
        app.UseRouting();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }


}