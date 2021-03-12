using System;

namespace Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders
{
    public class CustomResponseDataBuilder<T> : ResponseDataBuilderBase<T> where T : Exception
    {
        public Func<T, int> StatusCodeFunction { get; private set; }
        public Func<T, ResponseContext, object> BodyFunction { get; private set; }

        public CustomResponseDataBuilder()
        {
            StatusCodeFunction = GetDefaultStatusCode;
            BodyFunction = (exception, responseOptions) => new object();
        }

        public CustomResponseDataBuilder<T> WithStatusCode(Func<T, int> statusCodeFunction)
        {
            StatusCodeFunction = statusCodeFunction;
            return this;
        }
        public CustomResponseDataBuilder<T> WithStatusCode(int statusCode)
        {
            StatusCodeFunction = (exception) => statusCode;
            return this;
        }

        public CustomResponseDataBuilder<T> WithBody(Func<T, ResponseContext, object> bodyFunction)
        {
            BodyFunction = bodyFunction;
            return this;
        }

        public CustomResponseDataBuilder<T> WithBody(object body)
        {
            BodyFunction = (exception, options) => body;
            return this;
        }

        protected override int GetStatusCode(T exception) => StatusCodeFunction(exception);

        protected override object GetBody(T exception, ResponseContext options) => BodyFunction(exception, options);
    }
}
