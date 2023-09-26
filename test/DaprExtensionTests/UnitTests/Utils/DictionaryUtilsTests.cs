namespace DaprExtensionTests.UnitTests.Utils
{
    using System.Text.Json;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Xunit;

    public class DictionaryUtilsTests
    {
        [Fact]
        public void ToCaseInsensitiveDictionary_PositiveCases()
        {
            // Arrange
            var json = @"{
                ""Name"": ""John"",
                ""Age"": 30,
                ""City"": ""New York""
            }";

            var element = JsonDocument.Parse(json).RootElement;

            // Act
            var dictionary = element.ToCaseInsensitiveDictionary();

            // Assert
            Assert.Equal(3, dictionary.Count);
            Assert.True(dictionary.ContainsKey("name"));
            Assert.True(dictionary.ContainsKey("age"));
            Assert.True(dictionary.ContainsKey("city"));
            Assert.Equal("John", dictionary["name"].GetString());
            Assert.Equal(30, dictionary["age"].GetInt32());
            Assert.Equal("New York", dictionary["city"].GetString());
        }

        [Fact]
        public void ToCaseInsensitiveDictionary_EmptyElement()
        {
            // Arrange
            var json = @"{}";

            var element = JsonDocument.Parse(json).RootElement;

            // Act
            var dictionary = element.ToCaseInsensitiveDictionary();

            // Assert
            Assert.Empty(dictionary);
        }

        [Fact]
        public void ToCaseInsensitiveDictionary_NullValueProperties()
        {
            // Arrange
            var json = @"{
                ""Name"": null,
                ""Age"": null
            }";

            var element = JsonDocument.Parse(json).RootElement;

            // Act
            var dictionary = element.ToCaseInsensitiveDictionary();

            // Assert
            Assert.Empty(dictionary);
        }

        [Fact]
        public void ToCaseInsensitiveDictionary_NullElement()
        {
            // Arrange
            JsonElement element = default;

            // Act
            var dictionary = element.ToCaseInsensitiveDictionary();

            // Assert
            Assert.Empty(dictionary);
        }

        [Fact]
        public void ToCaseInsensitiveDictionary_DuplicateProperties()
        {
            // Arrange
            var json = @"{
                ""Name"": ""John"",
                ""name"": ""Doe"",
                ""Age"": 30
            }";

            var element = JsonDocument.Parse(json).RootElement;

            // Act
            var dictionary = element.ToCaseInsensitiveDictionary();

            // Assert
            Assert.Equal(2, dictionary.Count);
            Assert.True(dictionary.ContainsKey("name"));
            Assert.True(dictionary.ContainsKey("age"));
            Assert.Equal("Doe", dictionary["name"].GetString()); // The last occurrence should overwrite the previous one
            Assert.Equal(30, dictionary["age"].GetInt32());
        }
    }

}
