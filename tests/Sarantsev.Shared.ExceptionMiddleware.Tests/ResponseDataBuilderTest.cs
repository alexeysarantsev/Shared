using System;
using FluentAssertions;
using Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Sarantsev.Shared.ExceptionMiddleware.Tests
{
    public class ResponseDataBuilderTest
    {
        [Theory, AutoMoqData]
        public void GetResponseData_NoAdditionalSetup_ReturnsProblemDetails(string errorMessage)
        {
            //Arrange
            var responseDataBuilder = new ResponseDataBuilder<Exception, ProblemDetails>();
            var expectedData = new ResponseData()
            {
                StatusCode = 500,
                Body = new ProblemDetails()
                {
                    Status = 500,
                    Title = errorMessage
                }
            };

            //Act
            var responseData =
                responseDataBuilder.GetResponseOptions(new Exception(errorMessage), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_ConstPassed_ReturnsInProblemDetails(string errorMessage, int statusCode,
            string detail, string instance, string title, string type)
        {
            //Arrange
            var responseDataBuilder = new ResponseDataBuilder<Exception, ProblemDetails>();
            responseDataBuilder
                .WithStatusCode(statusCode)
                .WithDetail(detail)
                .WithInstance(instance)
                .WithTitle(title)
                .WithType(type);
            var expectedData = new ResponseData()
            {
                StatusCode = statusCode,
                Body = new ProblemDetails()
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Type = type,
                    Instance = instance
                }
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(errorMessage), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_FunctionPassed_ReturnsInProblemDetails(string errorMessage, int statusCode,
            string detail, string instance, string title, string type)
        {
            //Arrange
            var responseDataBuilder = new ResponseDataBuilder<Exception, ProblemDetails>();
            responseDataBuilder
                .WithStatusCode((e) => statusCode)
                .WithDetail((e) => detail)
                .WithInstance((e) => instance)
                .WithTitle((e) => title)
                .WithType((e) => type);
            var expectedData = new ResponseData()
            {
                StatusCode = statusCode,
                Body = new ProblemDetails()
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Type = type,
                    Instance = instance
                }
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(errorMessage), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_DuplicatedParameters_LastParametersTaken(string errorMessage,
            int statusCode1, string detail1, string instance1, string title1, string type1,
            int statusCode2, string detail2, string instance2, string title2, string type2
            )
        {
            //Arrange
            var responseDataBuilder = new ResponseDataBuilder<Exception, ProblemDetails>();
            responseDataBuilder
                .WithStatusCode(statusCode1)
                .WithDetail(detail1)
                .WithInstance(instance1)
                .WithTitle(title1)
                .WithType(type1)
                .WithStatusCode(statusCode2)
                .WithDetail(detail2)
                .WithInstance(instance2)
                .WithTitle(title2)
                .WithType(type2);
            var expectedData = new ResponseData()
            {
                StatusCode = statusCode2,
                Body = new ProblemDetails()
                {
                    Status = statusCode2,
                    Title = title2,
                    Detail = detail2,
                    Type = type2,
                    Instance = instance2
                }
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(errorMessage), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_ActionPassed_ActionCalled(string errorMessage, int statusCode, string testField)
        {
            //Arrange
            var responseDataBuilder = new ResponseDataBuilder<Exception, TestProblemDetails>();
            responseDataBuilder
                .WithStatusCode(statusCode)
                .WithAction((tpd, e) => { tpd.TestField = testField; });
            var expectedData = new ResponseData()
            {
                StatusCode = statusCode,
                Body = new TestProblemDetails()
                {
                    Status = statusCode,
                    Title = errorMessage,
                    TestField = testField
                }
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(errorMessage), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_IncludeDeveloperData_ReturnsProblemDetailsWithStackTrace
            (string errorMessage, string stackTrace, string path)
        {
            //Arrange
            var validPath = "/" + path;
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.SetupGet(r => r.Path).Returns(validPath);
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(r => r.Request).Returns(httpRequestMock.Object);

            var responseDataBuilder = new ResponseDataBuilder<Exception, ProblemDetails>();
            var expectedData = new ResponseData()
            {
                StatusCode = 500,
                Body = new ProblemDetails()
                {
                    Status = 500,
                    Instance = validPath,
                    Title = errorMessage,
                    Extensions =
                    {
                        ["stackTrace"] = stackTrace
                    }
                }
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(
                new StackTraceOverridenException(errorMessage, stackTrace), new ResponseContext
                {
                    IncludeDeveloperData = true,
                    HttpContext = httpContext.Object
                });

            //Assert
            responseData.StatusCode.Should().Be(expectedData.StatusCode);
            responseData.Body.Should().BeEquivalentTo(expectedData.Body);
        }

        private class TestProblemDetails : ProblemDetails
        {
            public string TestField { get; set; }
        }

        private class StackTraceOverridenException : Exception
        {
            public StackTraceOverridenException(string message, string stackTrace)
                : base(message)
            {
                StackTrace = stackTrace;
            }

            public override string StackTrace
            {
                get;
            }
        }
    }
}
