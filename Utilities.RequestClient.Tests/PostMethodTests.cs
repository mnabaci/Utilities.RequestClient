using AutoFixture;
using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Utilities.Serialization;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class PostMethodTests : TestBase
    {
        private const string Action = "post";

        [Fact]
        public void PostMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Post<ResultDto>(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.json.test);
        }

        [Fact]
        public void PostMethodWithNullActionShouldReturnNotFound()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Post<ResultDto>(null, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Null(result.Result);
        }

        [Fact]
        public void PostMethodWithNullBodyShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Post<ResultDto>(Action, null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }

        [Fact]
        public void PostMethodWithStringReturnTypeShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Post(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.Deserialize<ResultDto>().json.test);
        }

        [Fact]
        public void PostMethodWithoutBodyAndWithStringReturnTypeShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Post(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.Deserialize<ResultDto>().json);
        }

        [Fact]
        public void PostMethodWithoutBodyShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Post<ResultDto>(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }
    }
}