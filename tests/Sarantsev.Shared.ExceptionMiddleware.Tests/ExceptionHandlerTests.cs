using System;
using FluentAssertions;
using Xunit;

namespace Sarantsev.Shared.ExceptionMiddleware.Tests
{
    public class ExceptionHandlerTests
    {
        [Fact]
        public void GetResponseData_CallsOptionsBuilder()
        {
            //Arrange
            var funcWasCalled = false;

            ResponseData OptionsBuilder(Exception e, ResponseContext rc)
            {
                funcWasCalled = true;
                return null;
            }

            var exceptionHandler = new ExceptionHandler<Exception>(OptionsBuilder);

            //Act
            exceptionHandler.GetResponseData(new Exception(), new ResponseContext());

            //Assert
            funcWasCalled.Should().BeTrue();
        }

        [Fact]
        public void GetResponseDataGeneric_CallsOptionsBuilder()
        {
            //Arrange
            var funcWasCalled = false;

            ResponseData OptionsBuilder(Exception e, ResponseContext rc)
            {
                funcWasCalled = true;
                return null;
            }

            var exceptionHandler = new ExceptionHandler<Exception>(OptionsBuilder);

            //Act
            exceptionHandler.GetResponseDataGeneric(new Exception(), new ResponseContext());

            //Assert
            funcWasCalled.Should().BeTrue();
        }
    }
}
