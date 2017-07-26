﻿using FindIt.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace FindIt
{
	public partial class App : Application
    {
        public static ILocator Locator { get; private set; }

        public static IAuthenticate Authenticator { get; private set; }
        
        public static void Init(ILocator locator, IAuthenticate authenticator)
        {
            Locator = locator;
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
