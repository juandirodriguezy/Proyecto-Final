using AsistenteIA.Helpers;
using Xunit;

namespace AsistenteIA.Tests;

/// <summary>
/// Pruebas unitarias para el formateador de texto y limpiador de Markdown / LaTeX.
/// </summary>
public class TextFormatterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t  ")]
    public void CleanMarkdown_WithNullOrWhitespace_ReturnsEmptyString(string? input)
    {
        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void CleanMarkdown_WithBoldText_RemovesAsterisks()
    {
        // Arrange
        var input = "El resultado es **8** y es una suma **correcta**.";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Equal("El resultado es 8 y es una suma correcta.", result);
    }

    [Fact]
    public void CleanMarkdown_WithItalicText_RemovesAsterisks()
    {
        // Arrange
        var input = "Este concepto es *fundamental* para el cálculo.";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Equal("Este concepto es fundamental para el cálculo.", result);
    }

    [Fact]
    public void CleanMarkdown_WithHeadings_RemovesHashes()
    {
        // Arrange
        var input = "### Explicación formal\nContenido de la explicación.\n## Conclusión";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Contains("Explicación formal", result);
        Assert.Contains("Conclusión", result);
        Assert.DoesNotContain("#", result);
    }

    [Fact]
    public void CleanMarkdown_WithHorizontalRules_RemovesDashes()
    {
        // Arrange
        var input = "Sección A\n---\nSección B\n-----";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Contains("Sección A", result);
        Assert.Contains("Sección B", result);
        Assert.DoesNotContain("---", result);
    }

    [Fact]
    public void CleanMarkdown_WithLatexBlock_NormalizesContent()
    {
        // Arrange
        var input = "Fórmula:\n$$4 + 4 = 8$$\nFin.";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Contains("4 + 4 = 8", result);
        Assert.DoesNotContain("$$", result);
    }

    [Fact]
    public void CleanMarkdown_WithLatexInline_NormalizesContent()
    {
        // Arrange
        var input = "Sea $x$ una variable en el sistema.";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Equal("Sea x una variable en el sistema.", result);
        Assert.DoesNotContain("$", result);
    }

    [Fact]
    public void CleanMarkdown_WithLatexSymbols_TranslatesToSpanish()
    {
        // Arrange
        var input = @"Sea el conjunto $\mathbb{Z}$ y $\mathbb{R}$ con $\mathbb{N}$.";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Contains("números enteros", result);
        Assert.Contains("números reales", result);
        Assert.Contains("números naturales", result);
        Assert.DoesNotContain(@"\mathbb{", result);
    }

    [Fact]
    public void CleanMarkdown_WithExcessiveLineBreaks_NormalizesToDoubleBreak()
    {
        // Arrange
        var input = "Párrafo 1\n\n\n\n\nPárrafo 2";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.Equal("Párrafo 1\n\nPárrafo 2", result);
    }

    [Fact]
    public void CleanMarkdown_WithComplexGeminiResponse_ProducesCleanReadableText()
    {
        // Arrange: Simula la respuesta real de Gemini reportada por el usuario
        var input = "### Explicación formal\n\n" +
                    "En el conjunto de los $\\mathbb{Z}$:\n\n" +
                    "$$4 + 4 = 8$$\n\n" +
                    "**Demostración:**\n" +
                    "---\n" +
                    "El resultado aritmético es exacto.";

        // Act
        var result = TextFormatter.CleanMarkdown(input);

        // Assert
        Assert.DoesNotContain("###", result);
        Assert.DoesNotContain("$$", result);
        Assert.DoesNotContain("**", result);
        Assert.DoesNotContain("---", result);
        Assert.Contains("Explicación formal", result);
        Assert.Contains("números enteros", result);
        Assert.Contains("4 + 4 = 8", result);
        Assert.Contains("Demostración:", result);
    }

    [Fact]
    public void ExtractApiErrorMessage_WithArrayJsonError_ExtractsInnerMessage()
    {
        // Arrange: Exactamente el formato JSON de error 503 recibido por el usuario
        var json = """
        [
          {
            "error": {
              "code": 503,
              "message": "This model is currently experiencing high demand. Spikes in demand are usually temporary. Please try again later.",
              "status": "UNAVAILABLE"
            }
          }
        ]
        """;

        // Act
        var message = TextFormatter.ExtractApiErrorMessage(json);

        // Assert
        Assert.Equal("This model is currently experiencing high demand. Spikes in demand are usually temporary. Please try again later.", message);
        Assert.DoesNotContain("{", message);
        Assert.DoesNotContain("[", message);
    }

    [Fact]
    public void ExtractApiErrorMessage_WithObjectJsonError_ExtractsInnerMessage()
    {
        // Arrange: Formato estándar de error 429 (Resource Exhausted)
        var json = """
        {
          "error": {
            "code": 429,
            "message": "Resource has been exhausted (e.g. check quota).",
            "status": "RESOURCE_EXHAUSTED"
          }
        }
        """;

        // Act
        var message = TextFormatter.ExtractApiErrorMessage(json);

        // Assert
        Assert.Equal("Resource has been exhausted (e.g. check quota).", message);
        Assert.DoesNotContain("{", message);
    }

    [Fact]
    public void ExtractApiErrorMessage_WithPlainText_ReturnsTrimmedText()
    {
        // Arrange
        var text = "  Error de conexión desconocido con el servidor remoto.  ";

        // Act
        var result = TextFormatter.ExtractApiErrorMessage(text);

        // Assert
        Assert.Equal("Error de conexión desconocido con el servidor remoto.", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \t\n")]
    public void ExtractApiErrorMessage_WithNullOrWhitespace_ReturnsEmpty(string? input)
    {
        // Act
        var result = TextFormatter.ExtractApiErrorMessage(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractApiErrorMessage_WithMalformedJson_ReturnsTrimmedTextWithoutCrashing()
    {
        // Arrange
        var malformed = "{ error: incomplete json... ";

        // Act
        var result = TextFormatter.ExtractApiErrorMessage(malformed);

        // Assert
        Assert.Equal("{ error: incomplete json...", result);
    }
}
