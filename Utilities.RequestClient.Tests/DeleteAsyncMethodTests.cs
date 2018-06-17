using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Utilities.Serialization;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class DeleteAsyncMethodTests : TestBase
    {
        private const string Action = "delete";

        [Fact]
        public async void DeleteAsyncMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var queryString = "?test=test";
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).DeleteAsync<ResultDto>(Action + queryString);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.args.test);
        }

        [Fact]
        public async void DeleteAsyncMethodWithoutQueryStringShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).DeleteAsync<ResultDto>(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.args.test);
        }

        [Fact]
        public async void DeleteAsyncMethodWithNullActionShouldReturnNotFound()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).DeleteAsync<ResultDto>(null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Null(result.Result);
        }

        [Fact]
        public async void DeleteAsyncMethodWithStringReturnTypeShouldReturnValidResult()
        {
            //Arrange
            var queryString = "?test=test";
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).DeleteAsync($"{Action}{queryString}");

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.Deserialize<ResultDto>().args.test);
        }
    }
}