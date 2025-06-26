using Serilog;
using System.Text;

namespace Web.Host.Middleware
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Log incoming request details
                Log.Information("Handling Request: {Method} {Path} at {TimeUtc}",
                    context.Request.Method,
                    context.Request.Path,
                    DateTime.UtcNow);

                // Log Request Body
                var queryParameters = string.Join(", ", context.Request.Query.Select(q => $"{q.Key}={q.Value}"));
                if (!string.IsNullOrEmpty(queryParameters))
                {
                    Log.Information("Query Parameters: {QueryParameters}", queryParameters);
                }

                // Log Request Body for methods that include a body (POST, PUT, PATCH, etc.)
                if (context.Request.ContentLength > 0 || context.Request.Method == "OPTIONS" || context.Request.Method == "GET")
                {
                    context.Request.EnableBuffering();
                    string requestBodyContent = await ReadStreamInChunks(context.Request.Body);
                    Log.Information("Request Body: {RequestBody}", requestBodyContent);
                    context.Request.Body.Position = 0; // Reset the stream position for further processing
                }

                // Capture the original response stream
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Process the request
                await _next(context);

                // Read the response stream
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string responseBodyContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                // Log outgoing response details
                Log.Information("Finished Handling Request: {Method} {Path} responded {StatusCode} at {TimeUtc}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    DateTime.UtcNow);
                Log.Debug("Response Body: {ResponseBody}", responseBodyContent); // to display this, change appSetting Serilog Level from Information > Debug

                // Copy the response back to the original response stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (FormatException ex) // for custom exceptions that have been tagged as FormatException
            {
                Log.Error(ex, "User Input Error: {Method} {Path} at {TimeUtc}",
                    context.Request.Method,
                    context.Request.Path,
                    DateTime.UtcNow);

                Log.Error("Exception Details: {ExceptionMessage}", ex.Message);
                Log.Error("Stack Trace: {StackTrace}", ex.StackTrace);
            }
            catch (Exception ex) //catch all errors
            {
                Log.Error(ex, "An error occurred while processing the request: {Method} {Path} at {TimeUtc}",
                    context.Request.Method,
                    context.Request.Path,
                    DateTime.UtcNow);

                Log.Error("Exception Details: {ExceptionMessage}", ex.Message);
                Log.Error("Stack Trace: {StackTrace}", ex.StackTrace);
            }
        }

        private async Task<string> ReadStreamInChunks(Stream stream)
        {
            const int bufferSize = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            char[] buffer = new char[bufferSize];
            int read;
            var result = new StringBuilder();
            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                result.Append(buffer, 0, read);
            }
            return result.ToString();
        }
    }
}
