using Moq;
using Moq.Protected;
using SmhiWeatherApi.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SmhiWeatherApi.Tests
{
    public class SmhiServiceTests
    {
        private readonly Mock<ILogger<SmhiService>> _mockLogger = new();

        private SmhiService CreateService(Mock<HttpMessageHandler> mockHandler)
        {
            var httpClient = new HttpClient(mockHandler.Object);
            return new SmhiService(httpClient, _mockLogger.Object);
        }

        private Mock<HttpMessageHandler> SetupSuccessResponse(object response)
        {
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(response))
                });
            return mock;
        }

        private Mock<HttpMessageHandler> SetupErrorResponse()
        {
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });
            return mock;
        }

        [Fact]
        public async Task GetStationReadingAsync_ValidResponse_ReturnsStationReading()
        {
            // Arrange
            var timestamp = 1729417200000L;
            var temperatureResponse = new { value = new[] { new { date = timestamp, value = "15.2" } } };
            var windResponse = new { value = new[] { new { date = timestamp, value = "5.3" } } };

            var mock = new Mock<HttpMessageHandler>();
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("parameter/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(temperatureResponse))
                });

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("parameter/21")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(windResponse))
                });

            var service = CreateService(mock);

            // Act
            var result = await service.GetStationReadingAsync("159880");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(159880, result[0].StationId);
            Assert.Equal(15.2, result[0].Temperature);
            Assert.Equal(5.3, result[0].WindGust);
        }

        [Fact]
        public async Task GetStationReadingAsync_ApiReturnsError_ReturnsEmptyList()
        {
            // Arrange
            var mockHandler = SetupErrorResponse();
            var service = CreateService(mockHandler);

            // Act
            var result = await service.GetStationReadingAsync("159880");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Both temperature and wind calls fail, expect 2 error logs
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task GetStationReadingAsync_NoDataAvailable_ReturnsEmptyList()
        {
            // Arrange
            var emptyResponse = new { value = new object[0] };
            var mockHandler = SetupSuccessResponse(emptyResponse);
            var service = CreateService(mockHandler);

            // Act
            var result = await service.GetStationReadingAsync("159880");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid response")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
