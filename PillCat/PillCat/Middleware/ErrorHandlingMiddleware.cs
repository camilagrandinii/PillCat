using Newtonsoft.Json;
using ILogger = Serilog.ILogger;

namespace PillCat.Middleware
{
    /// <summary>
    /// Wraps all controller actions with a try-catch latch to avoid code repetition
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ErrorHandlingMiddleware(RequestDelegate next,
                                       ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoke Method, to validate requisition errors
        /// </summary>
        /// <param name="context"></param>
        public async Task InvokeAsync(HttpContext context)
        {
            var requestBody = string.Empty;
            context.Request.EnableBuffering();

            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = default;
            }
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(requestBody, context, ex);
            }
        }

        private async Task HandleExceptionAsync(string requestBody, HttpContext context, Exception exception)
        {
           
            
            _logger.Error(exception, "[{@user}] Error: {@exception}", context.Request.Headers["HEADER EXAMPLE"], exception.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
           

            _logger.Error(exception, "[traceId:{@traceId}]{@user} Error. Headers: {@headers}. Query: {@query}. Path: {@path}. Body: {@requestBody}",
                          context.TraceIdentifier, context.Request.Headers["HEADER EXAMPLE"],
                          context.Request.Headers, context.Request.Query, context.Request.Path, requestBody);

            await context.Response.WriteAsync(JsonConvert.SerializeObject($"{exception.Message}| traceId: {context.TraceIdentifier}"));
        }
    }
}
