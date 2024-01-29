namespace Smug.Middlewares;

public class QueryParamCheckMiddleware
{
    private readonly RequestDelegate _next;
    
    public QueryParamCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public QueryParamCheckMiddleware()
    {
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        string? queryParamValue = context.Request.Query["reason"];

        if (string.IsNullOrEmpty(queryParamValue))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Query parameter `reason` is missing");
            return;
        }

        await _next(context);
    }
}