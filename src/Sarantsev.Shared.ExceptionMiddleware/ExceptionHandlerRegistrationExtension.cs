using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    public static class ExceptionHandlerRegistrationExtension
    {
        public static ResponseDataBuilder<TE, TR> RegisterException<TE, TR>(this IServiceCollection services)
            where TE : Exception
            where TR : ProblemDetails, new()
        {
            var responseOptionsBuilder = new ResponseDataBuilder<TE, TR>();

            services.AddTransient<IExceptionHandler<TE>, ExceptionHandler<TE>>(s =>
                new ExceptionHandler<TE>(responseOptionsBuilder.GetResponseOptions));
            return responseOptionsBuilder;
        }

        public static ResponseDataBuilder<Exception, ProblemDetails>
            RegisterException(this IServiceCollection services) =>
            RegisterException<Exception, ProblemDetails>(services);

        public static ResponseDataBuilder<TE, ProblemDetails> RegisterException<TE>(this IServiceCollection services)
            where TE : Exception =>
            RegisterException<TE, ProblemDetails>(services);

        public static CustomResponseDataBuilder<TE> RegisterExceptionWithCustomResponse<TE>(
            this IServiceCollection services)
            where TE : Exception
        {
            var responseOptionsBuilder = new CustomResponseDataBuilder<TE>();
            services.AddTransient<IExceptionHandler<TE>, ExceptionHandler<TE>>(s =>
                new ExceptionHandler<TE>(responseOptionsBuilder.GetResponseOptions));
            return responseOptionsBuilder;
        }
    }

}
