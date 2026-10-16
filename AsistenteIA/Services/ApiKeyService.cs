namespace AsistenteIA.Services;

/// <summary>
/// Servicio para almacenamiento seguro de credenciales (API Keys) usando SecureStorage del sistema operativo.
/// Previene la exposición de claves en código duro, repositorios o archivos planos.
/// </summary>
public class ApiKeyService
{
    public const string GeminiKeyName = "gemini_api_key";
    public const string LegacyKeyName = "pollinations_api_key";

    /// <summary>
    /// Guarda de forma segura y encriptada la clave de API (por defecto para Google Gemini / Google AI Pro).
    /// </summary>
    public async Task SaveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía.", nameof(key));

        string cleanKey = key.Trim();
        await SecureStorage.Default.SetAsync(GeminiKeyName, cleanKey);
    }

    /// <summary>
    /// Obtiene la clave de API almacenada de forma segura. Retorna primero la de Gemini o la de respaldo si existe.
    /// </summary>
    public async Task<string?> GetAsync()
    {
        string? key = await SecureStorage.Default.GetAsync(GeminiKeyName);
        if (string.IsNullOrWhiteSpace(key))
        {
            key = await SecureStorage.Default.GetAsync(LegacyKeyName);
        }
        return key;
    }

    /// <summary>
    /// Elimina las claves de API del almacenamiento seguro.
    /// </summary>
    public bool Remove()
    {
        bool r1 = SecureStorage.Default.Remove(GeminiKeyName);
        bool r2 = SecureStorage.Default.Remove(LegacyKeyName);
        return r1 || r2;
    }

    /// <summary>
    /// Verifica si ya existe una clave configurada en el dispositivo.
    /// </summary>
    public async Task<bool> HasKeyAsync()
    {
        string? key = await GetAsync();
        return !string.IsNullOrWhiteSpace(key);
    }
}
