using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using TelerikFriendLocator.Managers;
using TelerikFriendLocator.Utilities;
using TelerikFriendLocator.Wrappers;

namespace TelerikFriendLocator.Pages
{
    public partial class StartPage : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher man;

        public StartPage()
        {
            GeneralManager.Instance.InsertTestData();
            InitializeComponent();
            man = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            GeneralManager.Instance.SetMap(map);
            GeneralManager.Instance.SetPage(this);

            man.StatusChanged += loc_StatusChanged;
        }

        private void loc_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
                {
                    GeneralManager.Instance.SetCoordinates();
                }
            }
        }

        public async void CallFbApi()
        {
            try
            {
                var result =
                    await
                        App.serviceClient.InvokeApiAsync("getfacebookfriends", HttpMethod.Get,
                            new Dictionary<string, string>
                            {
                                {"facebookId", App.serviceClient.CurrentUser.UserId}
                            });
            }
            catch (Exception e)
            {
            }
        }

      protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GeneralManager.Instance.GetUser();

            CallFbApi();
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                var result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                        "Location",
                        MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();

                GeneralManager.Instance.SetCoordinates();
            }
            else
            {
                GeneralManager.Instance.SetCoordinates();
            }
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
        }


        private void Map_OnLoaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "FriendLocator";
            MapsSettings.ApplicationContext.AuthenticationToken =
                "Ata6VjOFfYyG5vV9BHXJ45lDFLtbY9JLPHd45guVOSDbwdFstgQZKXHRDypQaMVA";
        }
    }
}