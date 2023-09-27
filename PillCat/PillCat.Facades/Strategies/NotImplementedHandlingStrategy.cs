using Microsoft.AspNetCore.Http;
using Serilog;

namespace PillCat.Facades.Strategies
{
    public class NotImplementedExceptionHandlingStrategy : ExceptionHandlingStrategy<NotImplementedException>
    {
        private readonly ILogger _logger;

        public NotImplementedExceptionHandlingStrategy(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<HttpContext> HandleAsync(HttpContext context, Exception exception)
        {
            var notImplementeException = ConvertToExceptionType(exception);
            _logger.Error(notImplementeException, "[{@user}] Error: {exception}", "Pillcat Header", notImplementeException.Message);
            context.Response.StatusCode = StatusCodes.Status501NotImplemented;

            return await Task.FromResult(context);
        }
    }
}
