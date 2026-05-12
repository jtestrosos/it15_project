using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using production_system.Services;
using Xunit;

namespace production_system.Tests.Services;

public class AesEncryptionServiceTests
{
    private readonly Mock<ILogger<AesEncryptionService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public AesEncryptionServiceTests()
    {
        _mockLogger = new Mock<ILogger<AesEncryptionService>>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    [Fact]
    public void Constructor_WithValidKey_DoesNotLogWarning()
    {
        // Arrange
        var validKeyBytes = RandomNumberGenerator.GetBytes(32);
        var validKeyBase64 = Convert.ToBase64String(validKeyBytes);
        
        _mockConfiguration.Setup(c => c["Encryption:AesKey"]).Returns(validKeyBase64);

        // Act
        var service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("No AES key found in configuration")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_WithoutKey_GeneratesKeyAndLogsWarning()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Encryption:AesKey"]).Returns((string?)null);

        // Act
        var service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Missing or invalid AES key")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithInvalidKeyFormatOrLength_GeneratesKeyAndLogsWarning()
    {
        // Arrange
        var invalidKeyBase64 = "not-base-64-and-wrong-length";
        
        _mockConfiguration.Setup(c => c["Encryption:AesKey"]).Returns(invalidKeyBase64);

        // Act
        var service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Missing or invalid AES key")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Hello World!")]
    [InlineData("1234567890")]
    [InlineData("SensitiveData123!@#")]
    public void EncryptAndDecrypt_ValidInput_ReturnsOriginalString(string plainText)
    {
        // Arrange
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        _mockConfiguration.Setup(c => c["Encryption:AesKey"]).Returns(Convert.ToBase64String(keyBytes));
        var service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);

        // Act
        string cipherText = service.Encrypt(plainText);
        string decryptedText = service.Decrypt(cipherText);

        // Assert
        Assert.NotEqual(plainText, cipherText);
        Assert.Equal(plainText, decryptedText);
    }

    [Fact]
    public void Encrypt_NullOrEmptyString_ReturnsSame()
    {
        // Arrange
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        _mockConfiguration.Setup(c => c["Encryption:AesKey"]).Returns(Convert.ToBase64String(keyBytes));
        var service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        Assert.Null(service.Encrypt(null!));
        Assert.Equal("", service.Encrypt(""));
    }

    [Fact]
    public void Decrypt_InvalidCipherText_ReturnsRawStringAndLogsError()
    {
        // Arrange
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        _mockConfiguration.Setup(c => c["Encryption:AesKey"]).Returns(Convert.ToBase64String(keyBytes));
        var service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);
        var invalidCipherText = "NotABase64String!";

        // Act
        var result = service.Decrypt(invalidCipherText);

        // Assert
        Assert.Equal(invalidCipherText, result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Failed to decrypt data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
