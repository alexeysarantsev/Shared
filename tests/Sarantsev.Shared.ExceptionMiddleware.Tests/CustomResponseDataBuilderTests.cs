using Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders;
using System;
using FluentAssertions;
using Xunit;

namespace Sarantsev.Shared.ExceptionMiddleware.Tests
{
    public class CustomResponseDataBuilderTests
    {
        [Fact]
        public void GetResponseData_NoAdditionalSetup_EmptyObject()
        {
            //Arrange
            var responseDataBuilder = new CustomResponseDataBuilder<Exception>();
            var expectedData = new ResponseData()
            {
                StatusCode = 500,
                Body = new object()
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_ObjectPassed_ObjectReturned(int statusCode, TestResponseObject testResponseObject)
        {
            //Arrange
            var responseDataBuilder = new CustomResponseDataBuilder<Exception>()
                .WithStatusCode(statusCode)
                .WithBody(testResponseObject);
            var expectedData = new ResponseData()
            {
                StatusCode = statusCode,
                Body = testResponseObject
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }

        [Theory, AutoMoqData]
        public void GetResponseData_FunctionPassed_ObjectReturned(int statusCode, TestResponseObject testResponseObject)
        {
            //Arrange
            var responseDataBuilder = new CustomResponseDataBuilder<Exception>()
                .WithStatusCode(e => statusCode)
                .WithBody((e, ro) => testResponseObject);
            var expectedData = new ResponseData()
            {
                StatusCode = statusCode,
                Body = testResponseObject
            };

            //Act
            var responseData = responseDataBuilder.GetResponseOptions(new Exception(), new ResponseContext());

            //Assert
            responseData.Should().BeEquivalentTo(expectedData);
        }
    }

    public class TestResponseObject
    {
        public string PropertyString { get; set; }
        public int PropertyInt { get; set; }
    }
}
