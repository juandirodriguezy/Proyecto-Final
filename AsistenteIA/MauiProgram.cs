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

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
