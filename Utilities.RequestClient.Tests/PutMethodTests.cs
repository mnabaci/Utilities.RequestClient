using AutoFixture;
using System.Net;
using Utilities.RequestClient.Tests.Base;
using Utilities.RequestClient.Tests.TestObjects;
using Utilities.Serialization;
using Xunit;

namespace Utilities.RequestClient.Tests
{
    public class PutMethodTests : TestBase
    {
        private const string Action = "put";

        [Fact]
        public void PutMethodWithValidParametersShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Put<ResultDto>(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.json.test);
        }

        [Fact]
        public void PutMethodWithNullActionShouldReturnNotFound()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Put<ResultDto>(null, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Null(result.Result);
        }

        [Fact]
        public void PutMethodWithNullBodyShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Put<ResultDto>(Action, null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }

        [Fact]
        public void PutMethodWithStringReturnTypeShouldReturnValidResult()
        {
            //Arrange
            var fixture = new Fixture();
            var requestDto = fixture.Build<RequestDto>()
                .With(x => x.test, "test")
                .Create();
            var expected = "test";

            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Put(Action, requestDto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Equal(expected, result.Result.Deserialize<ResultDto>().json.test);
        }

        [Fact]
        public void PutMethodWithoutBodyAndWithStringReturnTypeShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Put(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.Deserialize<ResultDto>().json);
        }

        [Fact]
        public void PutMethodWithoutBodyShouldReturnValidResult()
        {
            //Act
            var result = RequestClient.SetBaseUri(BaseUri).Put<ResultDto>(Action);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Result);
            Assert.Null(result.Result.json);
        }
    }
}