using AsistenteIA.Helpers;
using Xunit;

namespace AsistenteIA.Tests;

/// <summary>
/// Pruebas unitarias para las reglas de validación de prompts (RF02 / P01).
/// </summary>
public class PromptValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void IsValid_WithNullOrWhitespace_ReturnsFalse(string? prompt)
    {
        // Act
        bool isValid = PromptValidator.IsValid(prompt);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("abcd")]
    [InlineData("  1234  ")] // 4 caracteres útiles rodeados de espacios
    public void IsValid_WithLessThan5Characters_ReturnsFalse(string prompt)
    {
        // Act
        bool isValid = PromptValidator.IsValid(prompt);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("12345")] // Límite inferior exacto (5 caracteres)
    [InlineData("¿Cuánto es 4 + 4?")] // Prompt real simple
    [InlineData("Explica la normalización en tercera forma normal (3FN) para bases de datos relacionales.")]
    public void IsValid_WithValidPrompt_ReturnsTrue(string prompt)
    {
        // Act
        bool isValid = PromptValidator.IsValid(prompt);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithExactUpperBoundary2000Chars_ReturnsTrue()
    {
        // Arrange: 2000 caracteres exactos
        string prompt = new('a', 2000);

        // Act
        bool isValid = PromptValidator.IsValid(prompt);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithOver2000Characters_ReturnsFalse()
    {
        // Arrange: 2001 caracteres (excede el límite superior)
        string prompt = new('a', 2001);

        // Act
        bool isValid = PromptValidator.IsValid(prompt);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithShortPrompt_ReturnsDescriptiveError()
    {
        // Arrange
        string prompt = "hola";

        // Act
        var (isValid, errorMessage) = PromptValidator.Validate(prompt);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains("5", errorMessage);
    }

    [Fact]
    public void Validate_WithOverLimitPrompt_ReturnsDescriptiveError()
    {
        // Arrange
        string prompt = new('x', 2005);

        // Act
        var (isValid, errorMessage) = PromptValidator.Validate(prompt);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains("2000", errorMessage);
    }

    [Fact]
    public void Validate_WithValidPrompt_ReturnsSuccessWithoutError()
    {
        // Arrange
        string prompt = "Explica el algoritmo Dijkstra paso a paso.";

        // Act
        var (isValid, errorMessage) = PromptValidator.Validate(prompt);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }
}
