# SerilogElasticSearch [![Nuget](https://img.shields.io/nuget/v/Siganberg.SerilogElasticSearch)](https://www.nuget.org/packages/Siganberg.SerilogElasticSearch/) [![Nuget](https://img.shields.io/nuget/dt/Siganberg.SerilogElasticSearch)](https://www.nuget.org/packages/Siganberg.SerilogElasticSearch/)


## What is this library?

This library extend Serilog `UseSerilogRequestLogging` functionality. Using `UseSerilogRequestLogging` is already better than Microsoft way of logging HTTP request. Instead of writing HTTP request information like method, path, timing, status code and exception details in several events, `UseSerilogRequestLogging` collects information during the request (including from IDiagnosticContext) and writes a single event at request completion. This reduced chattiness of the logs.

Extended functionality:

- Hide or omit value in Request Header such as Authorization. Useful for hiding sensitive information in logs. 
- Control HTTP Request logging dynamically. For example, you don't want to log HTTP request logs from health check, metrics or swagger. Or you can use LaunchDarkly switch to control ON/OFF of HTTP request logging without restarting the application. 
- Ability to include or exclude HTTP request/response body or headers.
- Easily able to switch developer view logging or json logging (UseDeveloperView). Developer view logging is much easier to read during development. 
- Ability to control what elastic properties to include on the log or map to a custom name. Useful if your Kibana is using different index name. 

## Installation 

**First**, install the Siganberg.SerilogElasticSearch NuGet package into your app.

```console
dotnet add package Siganberg.SerilogElasticSearch
```

**Next**, in your application's _Program.cs_ file, configure Serilog.  


```c#
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.UseSerilog(); //-- Switch to Serilog with extended functionality
    
    builder.Services.AddControllers();

    //-- This is optional Interceptor to control when to log HTTP request. 
    builder.Services.AddSingleton<IRequestLoggingInterceptor, YourOwnRequestLoggingInterceptor>()
    

    var app = builder.Build();
    
    app.UseRequestLogging(); //-- For logging HTTP Request. Add this before any middleware
        
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
        
    app.UseRouting();

    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
```

**Then**, add `appsettings.json` configuration.

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  }
}
```

That's it. 




## Advance usage and configuration


Sample advance JSON configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "RequestLoggingOptions": {
      "IncludeResponseBody": "true",
      "IncludeRequestHeaders": "true",
      "ExcludeHeaderNames": [
        "Authorization",
        "Origin",
        "Referer"
      ]
    },
    "ElasticMappings": {
      "SourceContext": "callsite",
      "RequestMethod": "method"
    }
  }
}
```


*Serilog*

On top of default Serilog configuration, these are configuration for added functionality. 

| Property         | Default | Descriptions                                                                                                          |
|------------------|---------|-----------------------------------------------------------------------------------------------------------------------|
| UseDeveloperView | false   | Switch logging format to more compact and readable for developer.                                                     |                                                                                                                                               |
| ElasticMappings  | SourceContext : callsite <br>RequestMethod : method <br>Path : path <br>QueryString : queryString <br>StatusCode : responseStatus <br>Elapsed : durationMs <br>RequestHeaders : requestHeaders <br>RequestBody : requestBody <br>ResponseBody : responseBody <br>ContentType : contentType <br>ContentLength : contentLength <br>   | Control what properties to include and also there mapping name. You can use this to map to what name in Kibana index. |   

*RequestLoggingOptions*

| Property | Default | Descriptions                                                                                                                                       |
|---------------------|---------|----------------------------------------------------------------------------------------------------------------------------------------------------|
|     IncludeResponseBody                | false   | When true, it will add middleware to capture and add response body to the RequestLogging. This add overhead and should only use for troubleshooting if necessary.  |                                                                                                                                               |
|     IncludeRequestHeaders                | true   | Include Request Headers.  |
|     ExcludeHeaderNames                | empty   | Array of string of header names. Ignored if **IncludeRequestHeaders** is set to **false**. Header name will still be log but value is set to *\<OMITTED\>*  |
|     ExcludeHeaderNames                | empty   | Array of string of header names. Ignored if **IncludeRequestHeaders** is set to **false**. Header name will still be log but value is set to *\<OMITTED\>*  |




### Controlling RequestLogging dynamically 

By default, there is a registered `DefaultRequestLoggingInterceptor`. This interceptor automatically  filters out request logging for URLs containing either _health_, _swagger_, or _metrics_.

If you want to implement your own request logging interceptor create your own class that inherits from `IRequestLoggingInterceptor` and register it using dependency injection. See example above. 

*Note: This is only applicable for normal request flow. When ERROR occured, request logging are always injected since the goal of this logger is to get as much as information when something goes wrong.*

```c#
public class RequestLoggingInterceptor : IRequestLoggingInterceptor
{
    private readonly List<string> _excludedPaths = new List<string>
    {
        "healthz",
        "swagger"
    };

    public bool IncludeRequestWhen(HttpContext context)
    {
        var path = context.Request.Path.ToString();
        if (_excludedPaths.Any(a => path.Contains(a)))
            return false;

        //-- TODO: Add more logic here.  
        //-- example: Evaluate LaunchDarkly flag to turn ON/OFF RequestLogging based on some flag.

        return true;
    }    
}
  ```
