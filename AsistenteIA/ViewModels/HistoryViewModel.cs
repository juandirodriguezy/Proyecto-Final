using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsistenteIA.Models;
using AsistenteIA.Services;
using AsistenteIA.Views;

namespace AsistenteIA.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    public ObservableCollection<ConversationRecord> Conversations { get; } = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedCategory = "Todas";

    partial void OnSelectedCategoryChanged(string value)
    {
        _ = LoadConversationsAsync();
    }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasNoItems;

    public List<string> FilterCategories { get; } =
    [
        "Todas",
        "Código",
        "Bases de Datos",
        "Algoritmos",
        "Arquitectura",
        "General"
    ];

    public HistoryViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadConversationsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var results = await _databaseService.SearchConversationsAsync(SearchText, SelectedCategory);
            Conversations.Clear();
            foreach (var record in results)
            {
                Conversations.Add(record);
            }

            HasNoItems = Conversations.Count == 0;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo cargar el historial: {ex.Message}", "Aceptar");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FilterAsync()
    {
        await LoadConversationsAsync();
    }

    [RelayCommand]
    private async Task OpenDetailAsync(ConversationRecord record)
    {
        if (record is null) return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "Conversation", record }
        };

        await Shell.Current.GoToAsync(nameof(DetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteConversationAsync(ConversationRecord record)
    {
        if (record is null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar eliminación (RF06 / P08)",
            $"¿Desea eliminar la consulta:\n\n\"{record.ShortPrompt}\"?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirm) return;

        try
        {
            await _databaseService.DeleteConversationAsync(record);
            Conversations.Remove(record);
            HasNoItems = Conversations.Count == 0;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo eliminar el registro: {ex.Message}", "Aceptar");
        }
    }
}
