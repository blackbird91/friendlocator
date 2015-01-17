using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using TelerikFriendLocator.Wrappers;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Geolocation;

namespace TelerikFriendLocator.Managers
{
    public class PositionManager
    {

        public static readonly PositionManager Instance = new PositionManager();

        public async void SetCoordinates()
        {
            string facebookId = App.serviceClient.CurrentUser.UserId.Split(':')[1];

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                double latitude = geoposition.Coordinate.Latitude;
                double longitude = geoposition.Coordinate.Longitude;

                var userTable = App.serviceClient.GetTable<User>();
                var currentUser = await userTable.Where(u => u.FacebookId == facebookId).ToListAsync();

                currentUser[0].Latitude = (decimal)latitude;
                currentUser[0].Longitude = (decimal)longitude;

                await userTable.UpdateAsync(currentUser[0]);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("Location  is disabled in phone settings.","Press button to continue...",MessageBoxButton.OK);
                }
            }


        }
    }
}
