using Microsoft.AspNetCore.Http;
using RestEase;
using Serilog;

namespace PillCat.Facades.Strategies
{
    public class ApiExceptionHandlingStrategy : ExceptionHandlingStrategy<ApiException>
    {
        private readonly ILogger _logger;

        public ApiExceptionHandlingStrategy(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<HttpContext> HandleAsync(HttpContext context, Exception exception)
        {
            var apiException = ConvertToExceptionType(exception);
            _logger.Error(apiException, "[{@user}] Error: {exception}", "PillCat Header", apiException.Message);
            context.Response.StatusCode = (int)apiException.StatusCode;

            return await Task.FromResult(context);
        }
    }
}
