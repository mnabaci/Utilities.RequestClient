using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class GetMethodTests : TestBase
    {
        private const string Action = "get";

        [Fact]
        public void GetMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var queryString = "?test=test";
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Get<ResultDto>($"{Action}{queryString}");

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.args.test);
        }

        [Fact]
        public void GetMethodWithoutQueryStringShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Get<ResultDto>($"{Action}");

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.args.test);
        }

        [Fact]
        public void GetMethodWithNullActionShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Get(null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
        }
    }
}
