using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Utilities.Serialization;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class DeleteMethodTests : TestBase
    {
        private const string Action = "delete";

        [Fact]
        public void DeleteMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var queryString = "?test=test";
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Delete<ResultDto>(Action + queryString);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.args.test);
        }

        [Fact]
        public void DeleteMethodWithoutQueryStringShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Delete<ResultDto>(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.args.test);
        }

        [Fact]
        public void DeleteMethodWithNullActionShouldReturnNotFound()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Delete<ResultDto>(null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Null(result.Result);
        }

        [Fact]
        public void DeleteMethodWithStringReturnTypeShouldReturnValidResult()
        {
            //Arrange
            var queryString = "?test=test";
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Delete($"{Action}{queryString}");

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.Deserialize<ResultDto>().args.test);
        }
    }
}
