using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsistenteIA.Models;
using AsistenteIA.Services;

namespace AsistenteIA.ViewModels;

public class AcademicTemplate
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "📝";
    public string Category { get; set; } = "General";
    public string Text { get; set; } = string.Empty;
}

public partial class ChatViewModel : ObservableObject
{
    private readonly AiService _aiService;
    private readonly DatabaseService _databaseService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CharacterCountText))]
    [NotifyPropertyChangedFor(nameof(IsPromptValid))]
    private string prompt = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnswer))]
    private string answer = string.Empty;

    public bool HasAnswer => !string.IsNullOrWhiteSpace(Answer);

    [ObservableProperty]
    private string statusMessage = "Listo para formular consultas académicas.";

    [ObservableProperty]
    private string statusColor = "#64748B";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    private bool canSave;

    [ObservableProperty]
    private string selectedModel = AiService.DefaultModel;

    [ObservableProperty]
    private string selectedCategory = "Código";

    public string CharacterCountText => $"{Prompt?.Length ?? 0} / 2000 caracteres";

    public bool IsPromptValid => (Prompt?.Trim().Length ?? 0) >= 5 && (Prompt?.Trim().Length ?? 0) <= 2000;

    public List<AcademicTemplate> Templates { get; } =
    [
        new AcademicTemplate
        {
            Title = "Depuración de Código",
            Icon = "🐛",
            Category = "Código",
            Text = "Analiza el siguiente fragmento de código en [Lenguaje], identifica posibles errores sintácticos, de lógica o vulnerabilidades de seguridad, y explica cómo corregirlos paso a paso sin dar la solución completa de inmediato:\n\n```\n// Inserte su código aquí\n```"
        },
        new AcademicTemplate
        {
            Title = "Optimización SQL y BD",
            Icon = "🗄️",
            Category = "Bases de Datos",
            Text = "Explica cómo optimizar la siguiente consulta SQL para [PostgreSQL/MySQL/SQL Server] o sugiere el diseño relacional normalizado (3FN) para la siguiente necesidad académica:\n\n-- Inserte su consulta o entidad aquí\n"
        },
        new AcademicTemplate
        {
            Title = "Algoritmos y Big-O",
            Icon = "⚡",
            Category = "Algoritmos",
            Text = "Explica el funcionamiento y la complejidad temporal y espacial (notación Big-O) del siguiente algoritmo o estructura de datos con un ejemplo didáctico:\n\n"
        },
        new AcademicTemplate
        {
            Title = "Resumen Conceptual",
            Icon = "📖",
            Category = "General",
            Text = "Explica de forma clara, didáctica y con ejemplos aplicados los conceptos fundamentales de:\n\n"
        }
    ];

    public List<string> AvailableCategories { get; } =
    [
        "Código",
        "Bases de Datos",
        "Algoritmos",
        "Arquitectura",
        "General"
    ];

    public ChatViewModel(AiService aiService, DatabaseService databaseService)
    {
        _aiService = aiService;
        _databaseService = databaseService;
    }

    [RelayCommand]
    private void SelectTemplate(AcademicTemplate template)
    {
        if (template is null) return;
        Prompt = template.Text;
        SelectedCategory = template.Category;
        StatusMessage = $"Plantilla '{template.Title}' aplicada. Complete los campos entre corchetes.";
        StatusColor = "#0284C7"; // Info
    }

    [RelayCommand]
    private async Task AskAsync()
    {
        if (IsBusy) return;

        string cleanPrompt = (Prompt ?? string.Empty).Trim();

        // RF02: Validación rigurosa de longitud
        if (string.IsNullOrWhiteSpace(cleanPrompt) || cleanPrompt.Length < 5)
        {
            StatusMessage = "Validación (RF02): Ingrese una consulta de al menos 5 caracteres.";
            StatusColor = "#DC2626"; // Danger
            await Shell.Current.DisplayAlertAsync("Validación", "La consulta debe tener un mínimo de 5 caracteres.", "Aceptar");
            return;
        }

        if (cleanPrompt.Length > 2000)
        {
            StatusMessage = "Validación (RF02): La consulta supera los 2000 caracteres permitidos.";
            StatusColor = "#DC2626";
            await Shell.Current.DisplayAlertAsync("Validación", "La consulta no puede exceder los 2000 caracteres.", "Aceptar");
            return;
        }

        _cts = new CancellationTokenSource();
        IsBusy = true;
        CanSave = false;
        Answer = string.Empty;
        StatusMessage = "Enviando solicitud POST a la IA (esperando respuesta)...";
        StatusColor = "#0284C7"; // Info

        try
        {
            Answer = await _aiService.AskAsync(cleanPrompt, SelectedModel, _cts.Token);
            StatusMessage = "Respuesta recibida correctamente. Recuerde contrastar con fuentes confiables.";
            StatusColor = "#059669"; // Success
            CanSave = true;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Petición cancelada voluntariamente o tiempo de espera agotado (P05/P06).";
            StatusColor = "#D97706"; // Warning
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = $"Error de red / servicio: {ex.Message}";
            StatusColor = "#DC2626"; // Danger
            await Shell.Current.DisplayAlertAsync("Error de Conexión", ex.Message, "Aceptar");
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
            StatusColor = "#DC2626";
            await Shell.Current.DisplayAlertAsync("Configuración requerida", ex.Message, "Ir a Configuración");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error inesperado: {ex.Message}";
            StatusColor = "#DC2626";
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_cts is not null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            StatusMessage = "Cancelando solicitud en curso...";
            StatusColor = "#D97706";
        }
    }

    [RelayCommand]
    private async Task SaveToHistoryAsync()
    {
        if (string.IsNullOrWhiteSpace(Answer))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "No hay respuesta para guardar.", "Aceptar");
            return;
        }

        try
        {
            var record = new ConversationRecord
            {
                Prompt = Prompt.Trim(),
                Answer = Answer.Trim(),
                Category = SelectedCategory,
                CreatedAt = DateTime.UtcNow,
                IsVerified = false
            };

            await _databaseService.SaveConversationAsync(record);
            CanSave = false;
            StatusMessage = "Consulta guardada exitosamente en el historial local (SQLite).";
            StatusColor = "#059669";
            await Shell.Current.DisplayAlertAsync("Guardado", "La conversación ha sido registrada en el historial SQLite.", "Aceptar");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al guardar en SQLite: {ex.Message}";
            StatusColor = "#DC2626";
            await Shell.Current.DisplayAlertAsync("Error de Persistencia", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private void Clear()
    {
        Prompt = string.Empty;
        Answer = string.Empty;
        CanSave = false;
        StatusMessage = "Formulario limpiado. Listo para una nueva consulta.";
        StatusColor = "#64748B";
    }
}
