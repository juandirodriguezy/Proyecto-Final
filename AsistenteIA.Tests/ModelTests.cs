using System.Text.Json;
using AsistenteIA.Models;
using Xunit;

namespace AsistenteIA.Tests;

/// <summary>
/// Pruebas unitarias para los modelos de datos y serialización JSON.
/// </summary>
public class ModelTests
{
    [Fact]
    public void ChatRequest_Serialization_IncludesModelAndMessages()
    {
        // Arrange
        var request = new ChatRequest
        {
            Model = "gemini-2.5-pro",
            Messages =
            [
                new ChatMessage { Role = "system", Content = "Eres un tutor académico." },
                new ChatMessage { Role = "user", Content = "¿Qué es una clave primaria en SQL?" }
            ]
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<ChatRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("gemini-2.5-pro", deserialized.Model);
        Assert.Equal(2, deserialized.Messages.Count);
        Assert.Equal("system", deserialized.Messages[0].Role);
        Assert.Equal("¿Qué es una clave primaria en SQL?", deserialized.Messages[1].Content);
    }

    [Fact]
    public void ChatResponse_Deserialization_ParsesChoicesAndMessage()
    {
        // Arrange
        var json = """
        {
            "id": "chatcmpl-12345",
            "model": "gemini-2.5-pro",
            "choices": [
                {
                    "index": 0,
                    "message": {
                        "role": "assistant",
                        "content": "Una clave primaria identifica unívocamente cada fila."
                    },
                    "finish_reason": "stop"
                }
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ChatResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("chatcmpl-12345", response.Id);
        Assert.Equal("gemini-2.5-pro", response.Model);
        Assert.Single(response.Choices);
        Assert.Equal("assistant", response.Choices[0].Message.Role);
        Assert.Equal("Una clave primaria identifica unívocamente cada fila.", response.Choices[0].Message.Content);
        Assert.Equal("stop", response.Choices[0].FinishReason);
    }

    [Fact]
    public void ConversationRecord_ShortPrompt_WithShortText_ReturnsEntireText()
    {
        // Arrange
        var record = new ConversationRecord
        {
            Prompt = "¿Cuánto es 4 + 4?"
        };

        // Act & Assert
        Assert.Equal("¿Cuánto es 4 + 4?", record.ShortPrompt);
    }

    [Fact]
    public void ConversationRecord_ShortPrompt_WithExact80Chars_ReturnsEntireText()
    {
        // Arrange
        var text80 = new string('x', 80);
        var record = new ConversationRecord
        {
            Prompt = text80
        };

        // Act & Assert
        Assert.Equal(text80, record.ShortPrompt);
    }

    [Fact]
    public void ConversationRecord_ShortPrompt_WithMoreThan80Chars_TruncatesWithEllipsis()
    {
        // Arrange
        var text100 = new string('a', 100);
        var record = new ConversationRecord
        {
            Prompt = text100
        };

        // Act
        var shortPrompt = record.ShortPrompt;

        // Assert
        Assert.Equal(80, shortPrompt.Length);
        Assert.EndsWith("...", shortPrompt);
        Assert.StartsWith(new string('a', 77), shortPrompt);
    }

    [Fact]
    public void ConversationRecord_IsPendingVerification_ReflectsInverseOfIsVerified()
    {
        // Arrange
        var unverifiedRecord = new ConversationRecord { IsVerified = false };
        var verifiedRecord = new ConversationRecord { IsVerified = true };

        // Assert
        Assert.True(unverifiedRecord.IsPendingVerification);
        Assert.False(verifiedRecord.IsPendingVerification);
    }

    [Fact]
    public void ConversationRecord_DefaultValues_AreCorrectlyInitialized()
    {
        // Arrange
        var record = new ConversationRecord();

        // Assert
        Assert.Equal("General", record.Category);
        Assert.False(record.IsVerified);
        Assert.True(record.IsPendingVerification);
        Assert.Equal(string.Empty, record.Prompt);
        Assert.Equal(string.Empty, record.Answer);
        Assert.True((DateTime.UtcNow - record.CreatedAt).TotalSeconds < 5);
        Assert.False(string.IsNullOrWhiteSpace(record.FormattedDate));
    }
}
