using BlazorEmoji.Components;
using BlazorEmoji.Extensions;
using BlazorEmoji.Models;
using BlazorEmoji.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Moq.Protected;
using System.Text.Json;
using Xunit;

namespace BlazorEmoji.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEmojiServices_RegistersAllServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockJSRuntime = new Mock<IJSRuntime>();
            services.AddScoped<IJSRuntime>(_ => mockJSRuntime.Object);

            // Act
            services.AddEmojiServices("https://example.com/");
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<IEmojiService>());
            Assert.NotNull(serviceProvider.GetService<IRecentEmojiService>());
        }

        [Fact]
        public void AddEmojiServices_RegistersServicesAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddEmojiServices("https://example.com/");

            // Assert
            var emojiServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmojiService));
            var recentEmojiServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRecentEmojiService));

            Assert.Equal(ServiceLifetime.Scoped, emojiServiceDescriptor?.Lifetime);
            Assert.Equal(ServiceLifetime.Scoped, recentEmojiServiceDescriptor?.Lifetime);
        }

        [Fact]
        public void AddEmojiServices_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddEmojiServices("https://example.com/");

            // Assert
            Assert.Same(services, result);
        }
    }

    public class RecentEmojiServiceTests
    {
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly RecentEmojiService _service;

        public RecentEmojiServiceTests()
        {
            _mockJsRuntime = new Mock<IJSRuntime>();
            _service = new RecentEmojiService(_mockJsRuntime.Object);
        }

        [Fact]
        public async Task GetRecentAsync_ReturnsEmptyList_WhenNoDataInLocalStorage()
        {
            // Arrange
            _mockJsRuntime
                .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _service.GetRecentAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentAsync_ReturnsDeserializedEmojis_WhenDataExists()
        {
            // Arrange
            var emojis = new List<Emoji>
            {
                new() { Code = "1F600", Char = "😀", Name = "Grinning Face" },
                new() { Code = "1F604", Char = "😄", Name = "Smiling Face" }
            };
            var json = JsonSerializer.Serialize(emojis);

            _mockJsRuntime
                .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
                .ReturnsAsync(json);

            // Act
            var result = await _service.GetRecentAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("1F600", result[0].Code);
            Assert.Equal("😀", result[0].Char);
        }

        [Fact]
        public async Task GetRecentAsync_ReturnsEmptyList_OnJsonException()
        {
            // Arrange
            _mockJsRuntime
                .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
                .ReturnsAsync("invalid json");

            // Act
            var result = await _service.GetRecentAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddRecentAsync_AddsEmojiToFrontOfList()
        {
            // Arrange
            var existingEmojis = new List<Emoji>
            {
                new() { Code = "1F604", Char = "😄", Name = "Smiling Face" }
            };
            var existingJson = JsonSerializer.Serialize(existingEmojis);

            _mockJsRuntime
                .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
                .ReturnsAsync(existingJson);

            var newEmoji = new Emoji { Code = "1F600", Char = "😀", Name = "Grinning Face" };
            string? savedJson = null;

            // Mock the base InvokeAsync method for InvokeVoidAsync (which returns IJSVoidResult)
            _mockJsRuntime
                .Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
                .Callback<string, object[]>((method, args) => savedJson = args[1]?.ToString())
                .Returns(new ValueTask<IJSVoidResult>());

            // Act
            await _service.AddRecentAsync(newEmoji);

            // Assert
            Assert.NotNull(savedJson);
            var savedEmojis = JsonSerializer.Deserialize<List<Emoji>>(savedJson);
            Assert.NotNull(savedEmojis);
            Assert.Equal(2, savedEmojis.Count);
            Assert.Equal("1F600", savedEmojis[0].Code);
        }

        [Fact]
        public async Task AddRecentAsync_RemovesDuplicates()
        {
            // Arrange
            var emoji = new Emoji { Code = "1F600", Char = "😀", Name = "Grinning Face" };
            var existingEmojis = new List<Emoji>
            {
                new() { Code = "1F604", Char = "😄", Name = "Smiling Face" },
                emoji
            };
            var existingJson = JsonSerializer.Serialize(existingEmojis);

            _mockJsRuntime
                .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
                .ReturnsAsync(existingJson);

            string? savedJson = null;
            _mockJsRuntime
                .Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
                .Callback<string, object[]>((method, args) => savedJson = args[1]?.ToString())
                .Returns(new ValueTask<IJSVoidResult>());

            // Act
            await _service.AddRecentAsync(emoji);

            // Assert
            Assert.NotNull(savedJson);
            var savedEmojis = JsonSerializer.Deserialize<List<Emoji>>(savedJson);
            Assert.NotNull(savedEmojis);
            Assert.Equal(2, savedEmojis.Count); // Should not have duplicates
            Assert.Equal("1F600", savedEmojis[0].Code);
        }

        [Fact]
        public async Task AddRecentAsync_LimitsToMaximum24Emojis()
        {
            // Arrange
            var existingEmojis = Enumerable.Range(0, 24)
                .Select(i => new Emoji { Code = $"code{i}", Char = "😀", Name = $"Emoji {i}" })
                .ToList();
            var existingJson = JsonSerializer.Serialize(existingEmojis);

            _mockJsRuntime
                .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
                .ReturnsAsync(existingJson);

            var newEmoji = new Emoji { Code = "new", Char = "😎", Name = "New Emoji" };
            string? savedJson = null;

            _mockJsRuntime
                .Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
                .Callback<string, object[]>((method, args) => savedJson = args[1]?.ToString())
                .Returns(new ValueTask<IJSVoidResult>());

            // Act
            await _service.AddRecentAsync(newEmoji);

            // Assert
            Assert.NotNull(savedJson);
            var savedEmojis = JsonSerializer.Deserialize<List<Emoji>>(savedJson);
            Assert.NotNull(savedEmojis);
            Assert.Equal(24, savedEmojis.Count); // Should not exceed 24
            Assert.Equal("new", savedEmojis[0].Code); // New emoji should be first
            Assert.DoesNotContain(savedEmojis, e => e.Code == "code23"); // Last one should be removed
        }

        [Fact]
        public async Task ClearRecentAsync_RemovesDataFromLocalStorage()
        {
            // Arrange
            _mockJsRuntime
                .Setup(js => js.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()))
                .Returns(new ValueTask<IJSVoidResult>());

            // Act
            await _service.ClearRecentAsync();

            // Assert
            _mockJsRuntime.Verify(
                js => js.InvokeAsync<object>("localStorage.removeItem", It.Is<object[]>(args =>
                    args.Length == 1 && args[0].ToString() == "blazor-emoji-recents")),
                Times.Once);
        }
    }

    public class EmojiServiceTests
    {
        private readonly Mock<IRecentEmojiService> _mockRecentEmojiService;

        public EmojiServiceTests()
        {
            _mockRecentEmojiService = new Mock<IRecentEmojiService>();
        }

        [Fact]
        public async Task SearchAsync_ReturnsAllCategories_WhenQueryIsEmpty()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var emojiData = new
            {
                Categories = new[]
                {
                    new { Name = "Smileys & Emotion", Emojis = new[] { new { Code = "1F600", Char = "😀", Name = "Grinning Face", Keywords = new[] { "happy" } } } }
                }
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(emojiData))
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://example.com/") // Set base address
            };
            var service = new EmojiService(httpClient, _mockRecentEmojiService.Object);

            // Act
            var result = await service.SearchAsync("");

            // Assert
            Assert.Single(result);
            Assert.Equal("Smileys & Emotion", result[0].Name);
        }

        [Fact]
        public async Task SearchAsync_FiltersEmojisByName()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var emojiData = new
            {
                Categories = new[]
                {
                    new
                    {
                        Name = "Smileys & Emotion",
                        Emojis = new[]
                        {
                            new { Code = "1F600", Char = "😀", Name = "Grinning Face", Keywords = new[] { "happy" }, Category = (string?)null },
                            new { Code = "1F621", Char = "😡", Name = "Angry Face", Keywords = new[] { "mad" }, Category = (string?)null }
                        }
                    }
                }
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(emojiData))
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var service = new EmojiService(httpClient, _mockRecentEmojiService.Object);

            // Act
            var result = await service.SearchAsync("grinning");

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].Emojis);
            Assert.Equal("Grinning Face", result[0].Emojis[0].Name);
        }

        [Fact]
        public async Task SearchAsync_FiltersEmojisByKeywords()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var emojiData = new
            {
                Categories = new[]
                {
                    new
                    {
                        Name = "Smileys & Emotion",
                        Emojis = new[]
                        {
                            new { Code = "1F600", Char = "😀", Name = "Grinning Face", Keywords = new[] { "happy", "smile" }, Category = (string?)null },
                            new { Code = "1F621", Char = "😡", Name = "Angry Face", Keywords = new[] { "mad", "angry" }, Category = (string?)null }
                        }
                    }
                }
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(emojiData))
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var service = new EmojiService(httpClient, _mockRecentEmojiService.Object);

            // Act
            var result = await service.SearchAsync("happy");

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].Emojis);
            Assert.Equal("Grinning Face", result[0].Emojis[0].Name);
        }

        [Fact]
        public async Task GetRecentAsync_DelegatesToRecentEmojiService()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"categories\":[]}")
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var service = new EmojiService(httpClient, _mockRecentEmojiService.Object);

            var expectedEmojis = new List<Emoji>
            {
                new() { Code = "1F600", Char = "😀", Name = "Grinning Face" }
            };
            _mockRecentEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(expectedEmojis);

            // Act
            var result = await service.GetRecentAsync();

            // Assert
            Assert.Same(expectedEmojis, result);
            _mockRecentEmojiService.Verify(s => s.GetRecentAsync(), Times.Once);
        }

        [Fact]
        public async Task AddRecentAsync_DelegatesToRecentEmojiService()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"categories\":[]}")
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var service = new EmojiService(httpClient, _mockRecentEmojiService.Object);

            var emoji = new Emoji { Code = "1F600", Char = "😀", Name = "Grinning Face" };

            // Act
            await service.AddRecentAsync(emoji);

            // Assert
            _mockRecentEmojiService.Verify(s => s.AddRecentAsync(emoji), Times.Once);
        }
    }

    public class EmojiPickerTests : TestContext
    {
        private readonly Mock<IEmojiService> _mockEmojiService;

        public EmojiPickerTests()
        {
            _mockEmojiService = new Mock<IEmojiService>();
            Services.AddSingleton(_mockEmojiService.Object);
        }

        [Fact]
        public void EmojiPicker_DoesNotRender_WhenIsOpenIsFalse()
        {
            // Arrange
            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, false));

            // Assert
            Assert.DoesNotContain("emoji-picker-backdrop", cut.Markup);
        }

        [Fact]
        public void EmojiPicker_Renders_WhenIsOpenIsTrue()
        {
            // Arrange
            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));

            // Assert
            Assert.Contains("emoji-picker-backdrop", cut.Markup);
            Assert.Contains("emoji-picker", cut.Markup);
        }

        [Fact]
        public void EmojiPicker_DisplaysSearchBox()
        {
            // Arrange
            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));

            // Assert
            var searchInput = cut.Find("#emoji-search");
            Assert.NotNull(searchInput);
            Assert.Equal("Search emojis...", searchInput.GetAttribute("placeholder"));
        }

        [Fact]
        public void EmojiPicker_DisplaysRecentTab()
        {
            // Arrange
            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));

            // Assert
            var recentTab = cut.Find("[data-tab-name='recent']");
            Assert.NotNull(recentTab);
            Assert.Contains("active", recentTab.ClassName);
        }

        [Fact]
        public async Task EmojiPicker_SelectsEmoji_WhenEmojiClicked()
        {
            // Arrange
            var selectedEmoji = default(Emoji);
            var emoji = new Emoji { Code = "1F600", Char = "😀", Name = "Grinning Face" };

            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji> { emoji });

            _mockEmojiService
                .Setup(s => s.AddRecentAsync(It.IsAny<Emoji>()))
                .Returns(Task.CompletedTask);

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.OnEmojiSelected, e => { selectedEmoji = e; }));

            var emojiButton = cut.Find("[data-emoji-code='1F600']");
            await emojiButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

            // Assert
            Assert.NotNull(selectedEmoji);
            Assert.Equal("1F600", selectedEmoji.Code);
            _mockEmojiService.Verify(s => s.AddRecentAsync(It.IsAny<Emoji>()), Times.Once);
        }

        [Fact]
        public void EmojiPicker_DisplaysEmptyMessage_WhenNoRecentEmojis()
        {
            // Arrange
            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));

            // Assert
            Assert.Contains("No recent emojis yet", cut.Markup);
        }

        [Fact]
        public void EmojiPicker_DisplaysCategoryTabs()
        {
            // Arrange
            var categories = new List<EmojiCategory>
            {
                new() { Name = "Smileys & Emotion", Emojis = new List<Emoji>() },
                new() { Name = "Animals & Nature", Emojis = new List<Emoji>() }
            };

            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(categories);

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));

            // Assert
            Assert.Contains("Smileys & Emotion", cut.Find("[data-tab-name='Smileys & Emotion']").GetAttribute("title"));
            Assert.Contains("Animals & Nature", cut.Find("[data-tab-name='Animals & Nature']").GetAttribute("title"));
        }

        [Fact]
        public void EmojiPicker_HasAccessibilityAttributes()
        {
            // Arrange
            _mockEmojiService
                .Setup(s => s.GetAllCategoriesAsync(false))
                .ReturnsAsync(new List<EmojiCategory>());

            _mockEmojiService
                .Setup(s => s.GetRecentAsync())
                .ReturnsAsync(new List<Emoji>());

            // Act
            var cut = Render<EmojiPicker>(parameters => parameters
                .Add(p => p.IsOpen, true));

            // Assert
            var dialog = cut.Find(".emoji-picker");
            Assert.Equal("dialog", dialog.GetAttribute("role"));
            Assert.Equal("Emoji picker", dialog.GetAttribute("aria-label"));
            Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        }
    }
}
