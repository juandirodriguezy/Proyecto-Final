using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

/// <summary>
/// Servicio para el consumo de la API REST de Inteligencia Artificial (Pollinations / OpenAI Compatible).
/// </summary>
public class AiService
{
    private readonly HttpClient _client;
    private readonly ApiKeyService _keyService;

    public const string DefaultModel = "openai/gpt-5.4-nano";
    public const string AcademicSystemPrompt =
        "Eres un asistente académico universitario. Responde de forma clara, estructurada y pedagógica. " +
        "Explica conceptos paso a paso, fomenta el pensamiento crítico y la comprensión. " +
        "Si no tienes certeza absoluta sobre un dato o fuentes específicas, indícalo con transparencia y no inventes referencias.";

    public AiService(HttpClient client, ApiKeyService keyService)
    {
        _client = client;
        _keyService = keyService;
    }

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
            throw new InvalidOperationException("No se ha configurado la clave de API. Por favor, regístrela en la sección de Configuración.");

        string selectedModel = string.IsNullOrWhiteSpace(model) ? DefaultModel : model;

        var requestBody = new ChatRequest
        {
            Model = selectedModel,
            Messages =
            [
                new ChatMessage { Role = "system", Content = AcademicSystemPrompt },
                new ChatMessage { Role = "user", Content = cleanPrompt }
            ]
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
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
            throw new HttpRequestException($"Error de red al conectar con el servicio: {ex.Message}", ex);
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
                HttpStatusCode.Unauthorized => "Error 401: Clave de API no autorizada o inválida. Verifique en Configuración.",
                HttpStatusCode.Forbidden => "Error 403: Acceso denegado al modelo seleccionado.",
                HttpStatusCode.NotFound => "Error 404: Endpoint del servicio o modelo no encontrado.",
                HttpStatusCode.TooManyRequests => "Error 429: Se ha superado la cuota de peticiones permitidas.",
                HttpStatusCode.InternalServerError => "Error 500: Fallo interno en los servidores de la IA. Intente más tarde.",
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
            throw new InvalidOperationException("La respuesta del servicio no tiene el formato JSON esperado.", ex);
        }

        string? answer = result?.Choices.FirstOrDefault()?.Message.Content;
        return string.IsNullOrWhiteSpace(answer)
            ? "El servicio de IA respondió correctamente, pero no devolvió contenido de texto."
            : answer.Trim();
    }

    /// <summary>
    /// Realiza una prueba diagnóstica de conexión con una consulta mínima para validar credenciales y red.
    /// </summary>
    public async Task<(bool Success, string Message, long ElapsedMs)> TestConnectionAsync(string? model = null, CancellationToken token = default)
    {
        string? apiKey = await _keyService.GetAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, "No hay ninguna clave de API configurada en el dispositivo.", 0);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            string result = await AskAsync("Hola, responde únicamente con la palabra 'OK'.", model, token);
            stopwatch.Stop();
            return (true, $"Conexión exitosa. El servicio respondió: '{result}'", stopwatch.ElapsedMilliseconds);
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
