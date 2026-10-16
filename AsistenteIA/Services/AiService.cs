using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

/// <summary>
/// Servicio para el consumo de la API REST de Inteligencia Artificial enfocado en Google Gemini (Google AI Pro)
/// y compatible con endpoints OpenAI / Pollinations.
/// </summary>
public class AiService
{
    private readonly HttpClient _client;
    private readonly ApiKeyService _keyService;

    public const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";
    public const string PollinationsEndpoint = "https://gen.pollinations.ai/v1/chat/completions";

    public const string DefaultModel = "gemini-3.5-flash"; // Modelo activo y optimizado para Google Gemini

    public const string AcademicSystemPrompt =
        "Eres un asistente académico universitario de alto nivel impulsado por Google Gemini. " +
        "Responde de forma clara, directa, didáctica y pedagógica. " +
        "Adapta la extensión de tu respuesta a la complejidad de la pregunta: para consultas sencillas o directas, responde de forma concisa y sin rodeos innecesarios; para consultas complejas, explica paso a paso. " +
        "Escribe en texto plano natural y legible. " +
        "NO uses formato Markdown crudo como asteriscos dobles (**8**, **resultado**); escribe las palabras y números normalmente sin asteriscos. " +
        "NO uses sintaxis LaTeX (como $\\mathbb{Z}$, \\frac, o $$); escribe los símbolos matemáticos de manera natural (ej. 'números enteros', '4 + 4 = 8'). " +
        "Si no tienes certeza absoluta sobre un dato o fuente, indícalo con transparencia.";

    public AiService(HttpClient client, ApiKeyService keyService)
    {
        _client = client;
        _keyService = keyService;
    }

    /// <summary>
    /// Determina si un modelo pertenece a la familia Google Gemini.
    /// </summary>
    public static bool IsGeminiModel(string model) =>
        model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Retorna el endpoint REST correspondiente según el modelo seleccionado.
    /// </summary>
    public static string GetEndpointUrl(string model) =>
        IsGeminiModel(model) ? GeminiEndpoint : PollinationsEndpoint;

    /// <summary>
    /// Envía una pregunta o instrucción académica al modelo de IA y devuelve la respuesta generada.
    /// </summary>
    public async Task<string> AskAsync(string prompt, string? model = null, CancellationToken token = default)
    {
        string cleanPrompt = (prompt ?? string.Empty).Trim();

        // RF02: Validación de longitud obligatoria (entre 5 y 2000 caracteres)
        if (string.IsNullOrWhiteSpace(cleanPrompt) || cleanPrompt.Length < 5)
            throw new ArgumentException("La consulta es demasiado corta. Ingrese al menos 5 caracteres.");

        if (cleanPrompt.Length > 2000)
            throw new ArgumentException("La consulta supera el límite permitido de 2000 caracteres.");

        string? apiKey = await _keyService.GetAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("No se ha configurado la clave de API. Por favor, registre su API Key de Google Gemini en Configuración.");

        string selectedModel = string.IsNullOrWhiteSpace(model) ? DefaultModel : model;
        string endpointUrl = GetEndpointUrl(selectedModel);

        var requestBody = new ChatRequest
        {
            Model = selectedModel,
            Messages =
            [
                new ChatMessage { Role = "system", Content = AcademicSystemPrompt },
                new ChatMessage { Role = "user", Content = cleanPrompt }
            ]
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

        // Para Google Gemini, agregamos también la cabecera nativa x-goog-api-key para compatibilidad total
        if (IsGeminiModel(selectedModel))
        {
            request.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey.Trim());
        }

        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response;
        try
        {
            response = await _client.SendAsync(request, token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Error de red al conectar con Google Gemini: {ex.Message}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = string.Empty;
            try
            {
                errorContent = await response.Content.ReadAsStringAsync(token);
            }
            catch { }

            string friendlyMessage = response.StatusCode switch
            {
                HttpStatusCode.BadRequest => $"Error 400 (Parámetros inválidos o clave de Gemini no admitida): {errorContent}",
                HttpStatusCode.Unauthorized => "Error 401: Clave de API de Google Gemini no autorizada o inválida. Verifíquela en Google AI Studio (aistudio.google.com).",
                HttpStatusCode.Forbidden => "Error 403: Acceso restringido al modelo de Gemini seleccionado o permisos insuficientes en el proyecto de Google Cloud.",
                HttpStatusCode.NotFound => $"Error 404: El modelo '{selectedModel}' no fue encontrado en la API de Google Gemini.",
                HttpStatusCode.TooManyRequests => "Error 429: Cuota de peticiones excedida temporalmente. Espere unos momentos e intente nuevamente.",
                HttpStatusCode.InternalServerError => "Error 500: Fallo interno en los servidores de Google Gemini. Intente más tarde.",
                _ => $"Error del servicio ({response.StatusCode}): {errorContent}"
            };

            throw new HttpRequestException(friendlyMessage, null, response.StatusCode);
        }

        ChatResponse? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: token);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("La respuesta de Google Gemini no tiene el formato JSON esperado.", ex);
        }

        string? answer = result?.Choices.FirstOrDefault()?.Message.Content;
        if (string.IsNullOrWhiteSpace(answer))
        {
            return "Google Gemini respondió correctamente, pero no devolvió contenido de texto.";
        }

        return Helpers.TextFormatter.CleanMarkdown(answer);
    }

    /// <summary>
    /// Realiza una prueba diagnóstica de conexión con una consulta mínima para validar credenciales y red.
    /// </summary>
    public async Task<(bool Success, string Message, long ElapsedMs)> TestConnectionAsync(string? model = null, CancellationToken token = default)
    {
        string? apiKey = await _keyService.GetAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, "No hay ninguna clave de API configurada en el dispositivo.", 0);

        string testModel = string.IsNullOrWhiteSpace(model) ? DefaultModel : model;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            string result = await AskAsync("Hola, responde únicamente con la palabra 'OK'.", testModel, token);
            stopwatch.Stop();
            return (true, $"Conexión exitosa con {testModel} ({stopwatch.ElapsedMilliseconds} ms): '{result}'", stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return (false, "La prueba de conexión fue cancelada o superó el tiempo límite.", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return (false, ex.Message, stopwatch.ElapsedMilliseconds);
        }
    }
}
