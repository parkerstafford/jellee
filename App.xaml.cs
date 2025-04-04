using Microsoft.Maui.Controls;

namespace Jellee
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the RestaurantsPage as the main page of your app
            MainPage = new NavigationPage(new Pages.RestaurantsPage());
        }
    }
}
