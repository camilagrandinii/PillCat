using Microsoft.AspNetCore.Http;

namespace PillCat.Facades.Strategies.Interfaces
{
    public interface IExceptionHandlingStrategy
    {

        /// <summary>
        /// Handle the exception and return the corret StatusCode
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public Task<HttpContext> HandleAsync(HttpContext context, Exception exception);
    }
}
