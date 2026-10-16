using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToChatAsync()
    {
        await Shell.Current.GoToAsync("//ChatPage");
    }

    [RelayCommand]
    private async Task GoToHistoryAsync()
    {
        await Shell.Current.GoToAsync("//HistoryPage");
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }
}
