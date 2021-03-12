using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionMiddleware>();
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder, IHostingEnvironment env)
            => builder.UseMiddleware<ExceptionMiddleware>(env);

    }
}
