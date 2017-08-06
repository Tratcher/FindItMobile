using FindIt.Views;
using Plugin.Compass.Abstractions;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace FindIt
{
	public partial class App : Application
    {
        public static IGeolocator Locator { get; private set; }
        public static ICompass Compass { get; private set; }

        public static IAuthenticate Authenticator { get; private set; }
        
        public static void Init(IGeolocator locator, ICompass compass, IAuthenticate authenticator)
        {
            Locator = locator;
            Compass = compass;
            Authenticator = authenticator;
        }

        public App()
		{
			InitializeComponent();

			SetMainPage();
		}

        public static void SetMainPage()
        {
            Current.MainPage = new ItemsPage();
        }
    }
}
