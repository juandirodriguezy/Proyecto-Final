using AsistenteIA.ViewModels;

namespace AsistenteIA.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ChatViewModel vm)
        {
            vm.RefreshSelectedModel();
        }
    }
}
