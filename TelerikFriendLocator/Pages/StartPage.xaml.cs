using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;
using TelerikFriendLocator.Managers;

namespace TelerikFriendLocator.Pages
{

    public partial class StartPage : PhoneApplicationPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        public async void CallApi()
        {
            try
            {
                var result =
                    await
                        App.serviceClient.InvokeApiAsync("getfacebookfriends", HttpMethod.Get,
                            new Dictionary<string, string>()
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
            
            CallApi();

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            
            {
                MessageBoxResult result =
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

                PositionManager.Instance.SetCoordinates();
                //CallApi();
            }
            else
            {
                PositionManager.Instance.SetCoordinates();
            }
        }
    }
}