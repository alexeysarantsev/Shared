using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Sarantsev.Shared.ExceptionMiddleware.Tests
{
    public class ExceptionMiddlewareTests
    {
        [Theory, AutoMoqData]
        public async Task Invoke_NoException_DelegateCalled(NullLoggerFactory loggerFactory,
            Mock<IServiceProvider> serviceProviderMock)
        {
            //Arrange
            var delegateWasCalled = false;
            var requestDelegate = new RequestDelegate(_ =>
            {
                delegateWasCalled = true;
                return Task.CompletedTask;
            });
            var httpContext = new Mock<HttpContext>();
            var middleware = new ExceptionMiddleware(requestDelegate, loggerFactory, serviceProviderMock.Object);

            //Act
            await middleware.Invoke(httpContext.Object);

            //Assert
            delegateWasCalled.Should().BeTrue();
        }

        [Theory, AutoMoqData]
        public async Task Invoke_NoHandlersAdded_DefaultDelegateCalled(NullLoggerFactory loggerFactory,
            Mock<IServiceProvider> serviceProviderMock, string exceptionMessage,
            string path, string method)
        {
            //Arrange
            var requestDelegate = new RequestDelegate(httpContext => throw new Exception(exceptionMessage));

            var httpContextMock = new HttpContextMock();
            httpContextMock.HttpResponseMock.SetupSet(m => m.StatusCode = 500).Verifiable();
            httpContextMock.HttpResponseMock.SetupSet(m => m.ContentType = "application/problem+json").Verifiable();
            httpContextMock.HttpRequestMock.SetupGet(r => r.Path).Returns("/" + path);
            httpContextMock.HttpRequestMock.SetupGet(r => r.Method).Returns(method);
            var expectedResponse = new ProblemDetails()
            {
                Status = 500,
                Title = exceptionMessage
            };
            var expectedBody = JObject.FromObject(expectedResponse);

            var middleware = new ExceptionMiddleware(requestDelegate, loggerFactory, serviceProviderMock.Object);
            await middleware.Invoke(httpContextMock.Object);

            //Assert
            httpContextMock.HttpResponseMock.Verify();
            JToken.DeepEquals(expectedBody, JObject.Parse(httpContextMock.HttpResponseMock.ActualBody));
        }

        [Theory, AutoMoqData]
        public async Task Invoke_HandlerAdded_HandlerCalled(NullLoggerFactory loggerFactory,
            Mock<IServiceProvider> serviceProviderMock, string exceptionMessage, int statusCode,
            string path, string method)
        {
            var expectedResponse = new TestResponse
            {
                Message = exceptionMessage
            };
            var exceptionHandler = new Mock<IExceptionHandler<ArgumentException>>();
            exceptionHandler
                .Setup(eh => eh.GetResponseData(It.IsAny<Exception>(), It.IsAny<ResponseContext>()))
                .Returns((Exception exc, ResponseContext opt) => new ResponseData
                {
                    StatusCode = statusCode,
                    Body = expectedResponse
                });

            serviceProviderMock
                .Setup(s => s.GetService(typeof(IExceptionHandler<ArgumentException>)))
                .Returns(exceptionHandler.Object);

            //Arrange
            var requestDelegate = new RequestDelegate(httpContext => throw new ArgumentException(exceptionMessage));

            var httpContextMock = new HttpContextMock();
            httpContextMock.HttpResponseMock.SetupSet(m => m.StatusCode = statusCode).Verifiable();
            var expectedBody = JObject.FromObject(expectedResponse);
            httpContextMock.HttpRequestMock.SetupGet(r => r.Path).Returns("/" + path);
            httpContextMock.HttpRequestMock.SetupGet(r => r.Method).Returns(method);
            var middleware = new ExceptionMiddleware(requestDelegate, loggerFactory, serviceProviderMock.Object);

            //Act
            await middleware.Invoke(httpContextMock.Object);

            //Assert
            httpContextMock.HttpResponseMock.Verify();
            JToken.DeepEquals(expectedBody, JObject.Parse(httpContextMock.HttpResponseMock.ActualBody));
        }

        [Theory, AutoMoqData]
        public async Task Invoke_HandlerOfOtherTypeAdded_BaseDelegateCalled(NullLoggerFactory loggerFactory,
            Mock<IServiceProvider> serviceProviderMock, string exceptionMessage,
            string path, string method)
        {
            //Arrange
            var expectedResponse = new TestResponse
            {
                Message = exceptionMessage
            };

            var argumentExceptionHandler = new Mock<IExceptionHandler<ArgumentException>>();
            argumentExceptionHandler
                .Setup(eh => eh.GetResponseDataGeneric(It.IsAny<ArgumentException>(), It.IsAny<ResponseContext>()))
                .Returns((Exception exc, ResponseContext opt) => new ResponseData
                {
                    StatusCode = 400,
                    Body = null
                });

            var exceptionHandler = new Mock<IExceptionHandler<Exception>>();
            exceptionHandler
                .Setup(eh => eh.GetResponseData(It.IsAny<Exception>(), It.IsAny<ResponseContext>()))
                .Returns((Exception exc, ResponseContext opt) => new ResponseData
                {
                    StatusCode = 404,
                    Body = expectedResponse
                });

            serviceProviderMock
                .Setup(s => s.GetService(typeof(IExceptionHandler<ArgumentException>)))
                .Returns(argumentExceptionHandler.Object);
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IExceptionHandler<Exception>)))
                .Returns(exceptionHandler.Object);

            var requestDelegate =
                new RequestDelegate(httpContext => throw new InvalidOperationException(exceptionMessage));

            var httpContextMock = new HttpContextMock();
            httpContextMock.HttpResponseMock.SetupSet(m => m.StatusCode = 404).Verifiable();
            httpContextMock.HttpRequestMock.SetupGet(r => r.Path).Returns("/"+path);
            httpContextMock.HttpRequestMock.SetupGet(r => r.Method).Returns(method);
            var expectedBody = JObject.FromObject(expectedResponse);
            var middleware = new ExceptionMiddleware(requestDelegate, loggerFactory, serviceProviderMock.Object);

            //Act
            await middleware.Invoke(httpContextMock.Object);

            //Assert
            httpContextMock.HttpResponseMock.Verify();
            JToken.DeepEquals(expectedBody, JObject.Parse(httpContextMock.HttpResponseMock.ActualBody));
        }
    }

    internal class HttpContextMock : Mock<HttpContext>
    {
        private readonly Mock<IFeatureCollection> _featureCollectionMock = new Mock<IFeatureCollection>();

        public HttpResponseMock HttpResponseMock { get; private set; }

        public Mock<HttpRequest> HttpRequestMock { get; private set; }

        public HttpContextMock()
        {
            var httpResponseFeature = new Mock<IHttpResponseFeature>();
            _featureCollectionMock.Setup(f => f.Get<IHttpResponseFeature>()).Returns(httpResponseFeature.Object);
            SetupGet(h => h.Features).Returns(_featureCollectionMock.Object);
            HttpResponseMock = new HttpResponseMock(Object);
            SetupGet(h => h.Response).Returns(HttpResponseMock.Object);
            HttpRequestMock = new Mock<HttpRequest>();
            SetupGet(h => h.Request).Returns(HttpRequestMock.Object);
        }
    }

    internal class HttpResponseMock : Mock<HttpResponse>
    {
        private readonly HeaderDictionary _header = new HeaderDictionary();
        public HttpResponseMock(HttpContext httpContext)
        {
            SetupGet(h => h.Headers).Returns(_header);
            SetupGet(h => h.Body).Returns(Stream.Null);
            SetupGet(h => h.HttpContext).Returns(httpContext);
            SetupActualBody();
        }

        public string ActualBody { get; private set; }

        public void SetupActualBody()
        {
            Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Callback((byte[] data, int offset, int length, CancellationToken token) =>
            {
                if (length > 0)
                {
                    ActualBody = Encoding.UTF8.GetString(data);
                }
            });
        }
    }

    internal class TestResponse
    {
        public string Message { get; set; }
    }
}
