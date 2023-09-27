using Microsoft.AspNetCore.Http;
using PillCat.Facades.Strategies.Interfaces;

namespace PillCat.Facades.Strategies
{
    public abstract class ExceptionHandlingStrategy<T> : IExceptionHandlingStrategy where T : Exception
    {
        public T ConvertToExceptionType(Exception exception) => exception as T;

        public abstract Task<HttpContext> HandleAsync(HttpContext context, Exception exception);

    }
}
