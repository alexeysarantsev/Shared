using System;
using Microsoft.AspNetCore.Http;

namespace Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders
{
    public abstract class ResponseDataBuilderBase<T> where T : Exception
    {
        protected static int GetDefaultStatusCode(T exception) => StatusCodes.Status500InternalServerError;
        protected abstract int GetStatusCode(T exception);
        protected abstract object GetBody(T exception, ResponseContext options);
        public ResponseData GetResponseOptions(T exception, ResponseContext options) =>
            new ResponseData()
            {
                Body = GetBody(exception, options),
                StatusCode = GetStatusCode(exception)
            };
    }
}
