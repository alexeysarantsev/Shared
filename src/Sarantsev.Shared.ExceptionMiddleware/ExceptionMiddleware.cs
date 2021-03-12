using System;
using System.Threading.Tasks;
using Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IServiceProvider serviceProvider,
            IHostingEnvironment env = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger<ExceptionMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _serviceProvider = serviceProvider;
            _hostingEnvironment = env;
        }

        private async Task ProcessExceptionAsync(HttpContext context, Exception e, Type exceptionType)
        {
            var exceptionHandlerType = typeof(IExceptionHandler<>).MakeGenericType(exceptionType);
            if (_serviceProvider.GetService(exceptionHandlerType) is IExceptionHandler s)
            {
                var responseData = s.GetResponseData(e, GetResponseContext(context));
                await WriteResponseAsync(context, e, responseData);
            }
            else
            {
                var baseType = exceptionType.BaseType;
                if (baseType != null && baseType != typeof(object))
                {
                    await ProcessExceptionAsync(context, e, baseType);
                }
                else
                {
                    await WriteResponseAsync(context, e, GetDefaultResponseData(e, context));
                }
            }
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on calling {Method} {Path} {QueryString}", context.Request.Method,
                    context.Request.Path, context.Request.QueryString);
                await ProcessExceptionAsync(context, e, e.GetType());
            }
        }

        private async Task WriteResponseAsync(HttpContext context, Exception ex, ResponseData responseData)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the http status code middleware will not be executed.");
                throw ex;
            }
            context.Response.Clear();
            context.Response.StatusCode = responseData.StatusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(responseData.Body));
        }

        private ResponseData GetDefaultResponseData(Exception e, HttpContext httpContext) =>
            new ResponseDataBuilder<Exception, ProblemDetails>().GetResponseOptions(e, GetResponseContext(httpContext));

        private ResponseContext GetResponseContext(HttpContext httpContext)
        {
            return new ResponseContext()
            {
                HttpContext = httpContext,
                IncludeDeveloperData = _hostingEnvironment != null && _hostingEnvironment.IsDevelopment()
            };
        }
    }
}
