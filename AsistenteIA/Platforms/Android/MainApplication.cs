using Android.App;
using Android.Runtime;

namespace AsistenteIA;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp()
	{
		// Forzar inicialización temprana del proveedor nativo de SQLite para Android
		SQLitePCL.Batteries_V2.Init();
		return MauiProgram.CreateMauiApp();
	}
}
