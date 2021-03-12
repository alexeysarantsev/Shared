using System;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    public class ExceptionHandler<T> : IExceptionHandler<T> where T : Exception
    {
        private readonly Func<T, ResponseContext, ResponseData> _optionsBuilder;

        public ExceptionHandler(Func<T, ResponseContext, ResponseData> optionsBuilder)
        {
            _optionsBuilder = optionsBuilder;
        }

        public ResponseData GetResponseDataGeneric(T exception, ResponseContext responseContext) =>
            _optionsBuilder(exception, responseContext);

        public ResponseData GetResponseData(Exception exception, ResponseContext responseContext) =>
            GetResponseDataGeneric(exception as T, responseContext);
    }
}
