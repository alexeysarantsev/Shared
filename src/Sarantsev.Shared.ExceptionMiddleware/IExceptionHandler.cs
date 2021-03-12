using System;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    public interface IExceptionHandler
    {
        ResponseData GetResponseData(Exception exception, ResponseContext responseContext);
    }

    public interface IExceptionHandler<T> : IExceptionHandler where T : Exception
    {
        //it would be better to keep the same name 'GetResponseData'
        ResponseData GetResponseDataGeneric(T exception, ResponseContext responseContext);
    }
}
