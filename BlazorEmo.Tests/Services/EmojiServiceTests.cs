using Xunit;
using BlazorEmo.Services;
using Moq;
using Microsoft.JSInterop;
using Moq.Protected;
using System.Net;

public class EmoServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingEmojis()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""categories"": [
                        {
                            ""name"": ""Smileys & Emotion"",
                            ""emojis"": [
                                {
                                    ""code"": ""U+1F600"",
                                    ""char"": ""😀"",
                                    ""name"": ""grinning face"",
                                    ""keywords"": [""smile"", ""happy""]
                                },
                                {
                                    ""code"": ""U+1F60A"",
                                    ""char"": ""😊"",
                                    ""name"": ""smiling face with smiling eyes"",
                                    ""keywords"": [""smile"", ""blush""]
                                }
                            ]
                        }
                    ]
                }")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://test.com/")
        };
        var service = new EmoService(httpClient, new RecentEmoService(Mock.Of<IJSRuntime>()));

        // Act
        var results = await service.SearchAsync("smile");

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results.SelectMany(c => c.Emojis), e => 
            e.Name.Contains("smile", StringComparison.OrdinalIgnoreCase) ||
            e.Keywords.Any(k => k.Contains("smile", StringComparison.OrdinalIgnoreCase)));
    }
}