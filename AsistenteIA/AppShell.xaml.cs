using AsistenteIA.Views;

namespace AsistenteIA;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registro de ruta para la pantalla de detalle (Fase 9 de la Guía Maestra)
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
    }
}
