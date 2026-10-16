namespace AsistenteIA.Services;

/// <summary>
/// Servicio para almacenamiento seguro de credenciales (API Keys) usando SecureStorage del sistema operativo.
/// Previene la exposición de claves en código duro, repositorios o archivos planos.
/// </summary>
public class ApiKeyService
{
    private const string KeyName = "pollinations_api_key";

    /// <summary>
    /// Guarda de forma segura y encriptada la clave de API.
    /// </summary>
    public async Task SaveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía.", nameof(key));

        await SecureStorage.Default.SetAsync(KeyName, key.Trim());
    }

    /// <summary>
    /// Obtiene la clave de API almacenada de forma segura.
    /// </summary>
    public Task<string?> GetAsync() => SecureStorage.Default.GetAsync(KeyName);

    /// <summary>
    /// Elimina la clave de API del almacenamiento seguro.
    /// </summary>
    public bool Remove() => SecureStorage.Default.Remove(KeyName);

    /// <summary>
    /// Verifica si ya existe una clave configurada en el dispositivo.
    /// </summary>
    public async Task<bool> HasKeyAsync()
    {
        string? key = await GetAsync();
        return !string.IsNullOrWhiteSpace(key);
    }
}
