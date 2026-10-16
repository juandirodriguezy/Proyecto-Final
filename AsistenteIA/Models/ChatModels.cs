using System.Text.Json.Serialization;

namespace AsistenteIA.Models;

/// <summary>
/// Modelo de solicitud para la API de chat completions (compatible con OpenAI).
/// </summary>
public class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "openai/gpt-5.4-nano";

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];
}

/// <summary>
/// Mensaje individual dentro de la conversación con el modelo de IA.
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta devuelta por la API de chat completions.
/// </summary>
public class ChatResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = [];

    [JsonPropertyName("created")]
    public long? Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

/// <summary>
/// Opción generada por el modelo dentro de la respuesta.
/// </summary>
public class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}
