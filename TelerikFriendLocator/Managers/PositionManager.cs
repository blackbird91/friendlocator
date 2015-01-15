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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using System.Windows.Media;
using TelerikFriendLocator.Utilities;

namespace TelerikFriendLocator.Managers
{
    public class GeneralManager
    {

        public User LoggedInUser;
        public Map Map;
        public static readonly GeneralManager Instance = new GeneralManager();


        public GeneralManager()
        {
            GetUser();
        }

        public void SetMap(Map map)
        {
            Map = map;
        }
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

                LoggedInUser = currentUser[0];

                Map.MapElements.Clear();
                DrawRangeCircleOnMap(Map);

                GetUser();
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

        public void AddImageToMap(Map map,string facebookId,double latitude, double longitude, Uri imageUri, bool withEvent = false)
        {
            var mapIcon = new Image();
            mapIcon.Source = new BitmapImage(imageUri);
            mapIcon.Name = facebookId;
            mapIcon.Width = 20;
            mapIcon.Height = 20;

            MapLayer layer = new MapLayer();
            MapOverlay overlay = new MapOverlay();

            layer.Add(overlay);

            overlay.GeoCoordinate = new GeoCoordinate(latitude, longitude);
            overlay.Content = mapIcon;
            map.Layers.Add(layer);
        }

        private async void GetUser()
        {
            var facebookId = App.serviceClient.CurrentUser.UserId.Split(':')[1];

            var userTable = App.serviceClient.GetTable<User>();
            var currentUser = await userTable.Where(u => u.FacebookId == facebookId).ToListAsync();

            LoggedInUser = currentUser[0];
        }

        private void DrawRangeCircleOnMap(Map map)
        {
            map.MapElements.Clear();

            var location = new GeoCoordinate((double)LoggedInUser.Latitude, (double)LoggedInUser.Longitude);
            map.Center = location;
            map.ZoomLevel = 11;

            var fill = Colors.Purple;
            var stroke = Colors.Red;
            fill.A = 60;
            stroke.A = 60;
            var circle = new MapPolygon
            {
                StrokeThickness = 2,
                FillColor = fill,
                StrokeColor = stroke,
                StrokeDashed = false
            };

            foreach (var p in location.GetCirclePoints(LoggedInUser.Range < 200 ? 200 : (double)LoggedInUser.Range))
            {
                circle.Path.Add(p);
            }

            var fill2 = Colors.Red;
            var stroke2 = Colors.Black;
            fill2.A = 100;
            stroke2.A = 100;

            var circle2 = new MapPolygon
            {
                StrokeThickness = 3,
                FillColor = fill2,
                StrokeColor = stroke2,
                StrokeDashed = true
            };


            foreach (var p in location.GetCirclePoints(50))
            {
                circle2.Path.Add(p);
            }

            map.MapElements.Add(circle);
            map.MapElements.Add(circle2);
        }
    }
}
