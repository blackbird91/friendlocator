using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;

namespace TelerikFriendLocator
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Navigates to about page.
        /// </summary>
        private void GoToAbout(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }

        private async void LoginButton_OnTap(object sender, GestureEventArgs e)
        {
            await AuthenticateAsync();

            if (user != null && !string.IsNullOrEmpty(user.UserId))
            {
                NavigationService.Navigate(new Uri("/Pages/StartPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }


        private MobileServiceUser user;
        private async System.Threading.Tasks.Task AuthenticateAsync()
        {
            while (user == null)
            {
                string message;
                try
                {
                    user = await App.serviceClient
                        .LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                    message =
                        string.Format("You are now logged in - {0}", user.UserId);
                }
                catch (InvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }

            }
        }
    }
}
