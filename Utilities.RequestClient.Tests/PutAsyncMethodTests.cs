using System.Net;
using AutoFixture;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Utilities.Serialization;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class PutAsyncMethodTests : TestBase
    {
        private const string Action = "put";

        [Fact]
        public async void PutAsyncMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PutAsync<ResultDto>(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.json.test);
        }

        [Fact]
        public async void PutAsyncMethodWithNullActionShouldReturnNotFound()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PutAsync<ResultDto>(null, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Null(result.Result);
        }

        [Fact]
        public async void PutAsyncMethodWithNullBodyShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PutAsync<ResultDto>(Action, null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }

        [Fact]
        public async void PutAsyncMethodWithStringReturnTypeShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PutAsync(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.Deserialize<ResultDto>().json.test);
        }

        [Fact]
        public async void PutAsyncMethodWithoutBodyAndWithStringReturnTypeShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PutAsync(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.Deserialize<ResultDto>().json);
        }

        [Fact]
        public async void PutAsyncMethodWithoutBodyShouldReturnValidResult()
        {
            //Act
            var result = await RequestClient.SetBaseUri(BaseUri).PutAsync<ResultDto>(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }
    }
}
