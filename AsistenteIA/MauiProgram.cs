using Microsoft.Extensions.Logging;

namespace AsistenteIA;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Registro de servicios en el contenedor de dependencias (Fase 3)
		builder.Services.AddSingleton(new HttpClient
		{
			BaseAddress = new Uri("https://gen.pollinations.ai/v1/"),
			Timeout = TimeSpan.FromSeconds(60)
		});

		builder.Services.AddSingleton<Services.ApiKeyService>();
		builder.Services.AddSingleton<Services.AiService>();
		builder.Services.AddSingleton<Services.DatabaseService>();

		// Registro de ViewModels (Fases 5, 6 y 7)
		builder.Services.AddTransient<ViewModels.HomeViewModel>();
		builder.Services.AddTransient<ViewModels.ChatViewModel>();
		builder.Services.AddTransient<ViewModels.HistoryViewModel>();
		builder.Services.AddTransient<ViewModels.DetailViewModel>();
		builder.Services.AddTransient<ViewModels.SettingsViewModel>();

		// Registro de Vistas (Views)
		builder.Services.AddTransient<Views.HomePage>();
		builder.Services.AddTransient<Views.ChatPage>();
		builder.Services.AddTransient<Views.HistoryPage>();
		builder.Services.AddTransient<Views.DetailPage>();
		builder.Services.AddTransient<Views.SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
