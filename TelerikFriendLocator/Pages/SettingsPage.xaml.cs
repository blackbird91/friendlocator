using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TelerikFriendLocator.Wrappers;
using System.ComponentModel;
using TelerikFriendLocator.ViewModels;

namespace TelerikFriendLocator.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {

       
        private SettingsViewModel _ViewModel = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();

            LayoutRoot.DataContext = _ViewModel;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            NavigationService.Navigate(new Uri("/Pages/StartPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/StartPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private async void SaveSettings() 
        {
            string facebookId = App.serviceClient.CurrentUser.UserId.Split(':')[1];

            var userTable = App.serviceClient.GetTable<User>();
            var currentUser = await userTable.Where(u => u.FacebookId == facebookId).ToListAsync();

            currentUser[0].Range = (decimal)sliderRange.Value;
            currentUser[0].Visible = switchControl.IsChecked;

            await userTable.UpdateAsync(currentUser[0]);
        }

    }
}