using System.Net;
using System.Text;
using System.Text.Json;

public class ApiMiddleware
{
    private readonly RequestDelegate _next;

    public ApiMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Read the original response
            responseBody.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

            object data;
            try
            {
                // Try parse as JSON; if not, keep raw string
                data = JsonSerializer.Deserialize<object>(bodyText);
            }
            catch
            {
                data = bodyText;
            }

            var result = new
            {
                status = context.Response.StatusCode,
                data,
                success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300
            };

            var json = JsonSerializer.Serialize(result);

            // Write the wrapped JSON to the real response stream
            context.Response.Body = originalBodyStream;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                status = (int)HttpStatusCode.InternalServerError,
                data = ex.Message,
                success = false
            };

            var json = JsonSerializer.Serialize(errorResult);

            context.Response.Body = originalBodyStream;
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
            await context.Response.WriteAsync(json);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}