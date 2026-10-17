using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsistenteIA.Services;

namespace AsistenteIA.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ApiKeyService _apiKeyService;
    private readonly AiService _aiService;

    [ObservableProperty]
    private string apiKeyInput = string.Empty;

    [ObservableProperty]
    private bool hasSavedKey;

    [ObservableProperty]
    private string selectedModel = AiService.DefaultModel;

    [ObservableProperty]
    private List<string> availableModels =
    [
        "gemini-3.5-flash",
        "gemini-3.5-flash-lite",
        "gemini-3.8-flash",
        "gemini-3.1-pro-preview",
        "openai/gpt-5.4-nano",
        "mistral/mistral-large"
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotTestingConnection))]
    private bool isTestingConnection;

    public bool IsNotTestingConnection => !IsTestingConnection;

    public string ApiKeyPlaceholder => HasSavedKey
        ? "•••••••••••••••• (Clave configurada)"
        : "Pegue su clave de Google AI Studio";

    [ObservableProperty]
    private string diagnosticMessage = string.Empty;

    [ObservableProperty]
    private string diagnosticColor = "#64748B"; // TextMuted

    public SettingsViewModel(ApiKeyService apiKeyService, AiService aiService)
    {
        _apiKeyService = apiKeyService;
        _aiService = aiService;
    }

    /// <summary>
    /// Carga el estado inicial de la clave configurada al ingresar a la pantalla.
    /// </summary>
    [RelayCommand]
    public async Task InitializeAsync()
    {
        HasSavedKey = await _apiKeyService.HasKeyAsync();
        ApiKeyInput = string.Empty;
        OnPropertyChanged(nameof(ApiKeyPlaceholder));

        if (HasSavedKey)
        {
            DiagnosticMessage = "Clave de API de Google Gemini configurada en SecureStorage.";
            DiagnosticColor = "#059669"; // Success
        }
        else
        {
            DiagnosticMessage = "No hay ninguna clave registrada. Configure su API Key de Google Gemini (Google AI Pro) para habilitar las consultas.";
            DiagnosticColor = "#D97706"; // Warning
        }
    }

    [RelayCommand]
    private async Task SaveApiKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKeyInput))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Por favor ingrese una clave de API válida de Google Gemini.", "Aceptar");
            return;
        }

        try
        {
            await _apiKeyService.SaveAsync(ApiKeyInput.Trim());
            HasSavedKey = true;
            ApiKeyInput = string.Empty;
            OnPropertyChanged(nameof(ApiKeyPlaceholder));
            DiagnosticMessage = "Clave de API de Google Gemini guardada exitosamente en el almacenamiento seguro (SecureStorage).";
            DiagnosticColor = "#059669";
            await Shell.Current.DisplayAlertAsync("Éxito", "Clave de API de Google Gemini guardada de forma segura.", "Aceptar");
        }
        catch (Exception ex)
        {
            DiagnosticMessage = $"Error al guardar clave: {ex.Message}";
            DiagnosticColor = "#DC2626";
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task DeleteApiKeyAsync()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmación",
            "¿Está seguro de que desea eliminar la clave de API de Google Gemini del dispositivo?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirm) return;

        bool removed = _apiKeyService.Remove();
        if (removed || !await _apiKeyService.HasKeyAsync())
        {
            HasSavedKey = false;
            ApiKeyInput = string.Empty;
            OnPropertyChanged(nameof(ApiKeyPlaceholder));
            DiagnosticMessage = "La clave ha sido eliminada del almacenamiento seguro.";
            DiagnosticColor = "#D97706";
            await Shell.Current.DisplayAlertAsync("Eliminada", "Clave eliminada correctamente.", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsTestingConnection) return;

        IsTestingConnection = true;
        DiagnosticMessage = $"Probando conexión con {SelectedModel} (Google Gemini)...";
        DiagnosticColor = "#0284C7"; // Info

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            var (success, message, elapsedMs) = await _aiService.TestConnectionAsync(SelectedModel, cts.Token);

            if (success)
            {
                DiagnosticMessage = $"Prueba exitosa ({elapsedMs} ms): {message}";
                DiagnosticColor = "#059669"; // Success
            }
            else
            {
                DiagnosticMessage = $"Fallo en la prueba de conexión: {message}";
                DiagnosticColor = "#DC2626"; // Danger
            }
        }
        catch (Exception ex)
        {
            DiagnosticMessage = $"Error inesperado durante la prueba: {ex.Message}";
            DiagnosticColor = "#DC2626";
        }
        finally
        {
            IsTestingConnection = false;
        }
    }
}
