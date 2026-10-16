using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsistenteIA.Models;
using AsistenteIA.Services;

namespace AsistenteIA.ViewModels;

[QueryProperty(nameof(Conversation), "Conversation")]
public partial class DetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ConversationRecord? conversation;

    [ObservableProperty]
    private bool isVerified;

    [ObservableProperty]
    private string verificationBadgeText = "Pendiente de verificación";

    [ObservableProperty]
    private string verificationBadgeColor = "#D97706"; // Warning

    public DetailViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    partial void OnConversationChanged(ConversationRecord? value)
    {
        if (value is not null)
        {
            IsVerified = value.IsVerified;
            UpdateVerificationBadge();
        }
    }

    private void UpdateVerificationBadge()
    {
        if (IsVerified)
        {
            VerificationBadgeText = "✓ Verificada con fuentes académicas confiables";
            VerificationBadgeColor = "#059669"; // Success
        }
        else
        {
            VerificationBadgeText = "⚠️ Pendiente de verificación / Contrastar fuentes";
            VerificationBadgeColor = "#D97706"; // Warning
        }
    }

    [RelayCommand]
    private async Task ToggleVerificationAsync()
    {
        if (Conversation is null) return;

        IsVerified = !IsVerified;
        Conversation.IsVerified = IsVerified;
        UpdateVerificationBadge();

        try
        {
            await _databaseService.UpdateConversationAsync(Conversation);
            string feedback = IsVerified
                ? "Consulta marcada como verificada. ¡Excelente práctica ética!"
                : "Consulta marcada como pendiente de verificación.";
            await Shell.Current.DisplayAlertAsync("Actualización", feedback, "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo actualizar: {ex.Message}", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Conversation is null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar eliminación",
            "¿Desea eliminar esta consulta del historial definitivamente?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirm) return;

        try
        {
            await _databaseService.DeleteConversationAsync(Conversation);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo eliminar: {ex.Message}", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
