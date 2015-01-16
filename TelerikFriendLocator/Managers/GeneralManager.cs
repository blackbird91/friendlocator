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
using System.Threading;
using System.Windows.Threading;
using System.Net.Http;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Windows.Navigation;

namespace TelerikFriendLocator.Managers
{
    public class GeneralManager
    {

        public Page CurrentPage;
        public User LoggedInUser;
        public Map Map;

        public User  Person2;

        public static readonly GeneralManager Instance = new GeneralManager();

        public GeneralManager()
        {
            GetUser(true);

            DispatcherTimer timer = new DispatcherTimer();

            timer.Tick += timer_Tick;

            timer.Interval = new TimeSpan(0,0,30);
            timer.Start();

            //CallGetInRangeFriends();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            GetUser(true);
            SetCoordinates();
        }

        public void SetMap(Map map)
        {
            Map = map;
        }

        public void SetPage(Page page)
        {
            CurrentPage = page;
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

        public void AddImageToMap(Map map,User user, bool withEvent = false)
        {
            var mapIcon = new Image();
            mapIcon.Source = new BitmapImage(new Uri(user.PictureUrl));
            mapIcon.Name = user.FacebookId;
            mapIcon.Width = 75;
            mapIcon.Height = 75;

            MapLayer layer = new MapLayer();
            MapOverlay overlay = new MapOverlay();

            if(withEvent)
                mapIcon.Tap += ((sender, e) => mapIcon_Tap(sender, e, user)); 

            layer.Add(overlay);

            overlay.GeoCoordinate = new GeoCoordinate((double)user.Latitude, (double) user.Longitude);
            overlay.Content = mapIcon;
            map.Layers.Add(layer);
        }

        private void mapIcon_Tap(object sender, System.Windows.Input.GestureEventArgs e,User user)
        {
            Person2 = user;
            CurrentPage.NavigationService.Navigate(new Uri("/Messages.xaml", UriKind.RelativeOrAbsolute));
        }

        public async void GetUser(bool callDrawing = false)
        {
            var facebookId = App.serviceClient.CurrentUser.UserId.Split(':')[1];

            var userTable = App.serviceClient.GetTable<User>();
            var currentUser = await userTable.Where(u => u.FacebookId == facebookId).ToListAsync();

            LoggedInUser = currentUser[0];

            if (callDrawing)
            {
                DrawRangeCircleOnMap(Map);
                CallGetInRangeFriends();
            }
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

        public async void CallGetInRangeFriends()
        {
            try
            {
                var result =
                    await
                        App.serviceClient.InvokeApiAsync("getinrangefriends", HttpMethod.Get,
                            new Dictionary<string, string>
                            {
                                {"facebookId", App.serviceClient.CurrentUser.UserId},
                                {"range",LoggedInUser.Range.ToString()},
                                {"latitude",LoggedInUser.Latitude.ToString()},
                                {"longitude",LoggedInUser.Longitude.ToString()}
                            });



                ResponseWrapper friendsInRange = JsonConvert.DeserializeObject<ResponseWrapper>(result.ToString());

                foreach (var friend in friendsInRange.FriendsInRange)
                {
                    GeneralManager.Instance.AddImageToMap(Map, friend, true);
                }

            }
            catch (Exception e)
            {
            }

        }

        public async void InsertTestData()
        {
            try
            {

                var userTable = App.serviceClient.GetTable<User>();
                await userTable.UpdateAsync(new User()
                {
                    Id = "EDA01E07-253B-4B24-8159-5EB0EA257731",
                    PictureUrl = "https://www.facebook.com/l.php?u=https%3A%2F%2Ffbcdn-profile-a.akamaihd.net%2Fhprofile-ak-xap1%2Fv%2Ft1.0-1%2Fp50x50%2F10264979_990900747593761_6778829088706023510_n.jpg%3Foh%3D3b3babdf31f375198e57b36fd49c6e94%26oe%3D552E2E10%26__gda__%3D1429313062_cd6cd8c24ce058d97616da579f4b2cfc&h=PAQH0p_fL",
                    FacebookId = "930871800263323",
                    Firstname = "Radu",
                    Lastname = "Ungureanu",
                    Latitude = (decimal)44.430744,
                    Longitude = (decimal)26.154045,
                    Visible = true
                    
                });

                await userTable.UpdateAsync(new User()
                {
                    Id = "2A35ECB8-482F-4355-946A-2AF7AE77EA8F",
                    PictureUrl = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-xap1/v/t1.0-1/c12.0.50.50/p50x50/10344804_10204073422017032_8913957905083856757_n.jpg?oh=4180144a88fde00911053c1bce04239d&oe=555E24EA&__gda__=1433035177_6726be5c9aa8847485ea62d94d94a50c",
                    FacebookId = "1269057973",
                    Firstname = "Andreea",
                    Lastname = "Florescu",
                    Latitude = (decimal)44.44,
                    Longitude = (decimal)26.09,
                    Visible = true

                });

                await userTable.UpdateAsync(new User()
                {
                    Id = "620F9CF2-6BF6-40B9-B1BF-6E51AB63B0E8",
                    PictureUrl = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-xfa1/v/t1.0-1/c29.29.367.367/s50x50/216720_267966406552250_3331449_n.jpg?oh=babecbe9df1242538b94ecd7f35d80f4&oe=5521A52E&__gda__=1429549480_e2a1565dc0dbca296aa908f446570f81",
                    FacebookId = "100000166782587",
                    Firstname = "Mircea",
                    Lastname = "Moise",
                    Latitude = (decimal)44.435744,
                    Longitude = (decimal)26.114045,
                    Visible = true

                });

                await userTable.UpdateAsync(new User()
                {
                    Id="7CD61C77-2BD9-498C-9825-AB1BA97731C0",
                    PictureUrl = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-xpa1/v/t1.0-1/p50x50/10868135_10152970257042988_9076515268253969457_n.jpg?oh=b756ac895999c8b482d6ec978a977646&oe=55251DA9&__gda__=1432787988_437c50b743417020068a3fc6c5543ccb",
                    FacebookId = "517137987",
                    Firstname = "Adrian",
                    Lastname = "Chitescu",
                    Latitude = (decimal)44.450744,
                    Longitude = (decimal)26.134045,
                    Visible = true

                });

            }
            catch (Exception e)
            {
            }

        }
    }
}
