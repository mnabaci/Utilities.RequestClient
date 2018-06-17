using AutoFixture;
using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Utilities.Serialization;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class PostAsyncMethodTests : TestBase
    {
        private const string Action = "post";

        [Fact]
        public async void PostAsyncMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PostAsync<ResultDto>(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.json.test);
        }

        [Fact]
        public async void PostAsyncMethodWithNullActionShouldReturnNotFound()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PostAsync<ResultDto>(null, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Null(result.Result);
        }

        [Fact]
        public async void PostAsyncMethodWithNullBodyShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PostAsync<ResultDto>(Action, null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }

        [Fact]
        public async void PostAsyncMethodWithStringReturnTypeShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PostAsync(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.Deserialize<ResultDto>().json.test);
        }

        [Fact]
        public async void PostAsyncMethodWithoutBodyAndWithStringReturnTypeShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PostAsync(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.Deserialize<ResultDto>().json);
        }

        [Fact]
        public async void PostAsyncMethodWithoutBodyShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PostAsync<ResultDto>(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }
    }
}