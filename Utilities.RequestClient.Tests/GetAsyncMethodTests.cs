using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class GetAsyncMethodTests : TestBase
    {
        private const string Action = "get";

        [Fact]
        public async void GetAsyncMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var queryString = "?test=test";
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).GetAsync<ResultDto>($"{Action}{queryString}");

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.args.test);
        }

        [Fact]
        public async void GetAsyncMethodWithoutQueryStringShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).GetAsync<ResultDto>($"{Action}");

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.args.test);
        }

        [Fact]
        public async void GetAsyncMethodWithNullActionShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).GetAsync(null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
        }
    }
}
