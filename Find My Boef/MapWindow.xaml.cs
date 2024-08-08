using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using Find_My_Boef.Model;
using Find_My_Boef.View;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private bool isMakingOffense;
        private WebSocketStatusDataContext _status = new();

        public static Notifier Notifier = new(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public MapWindow()
        {
            Notifier = new(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
            // cant do async connect so connect before the window shows
            MainInstance.Client.SocketConnect();
            InitializeComponent();
            AdminVisibility();
            new TwitterApi();
            MainInstance.Timer += CheckLogin;

            if (!Database.IsOpen)
            {
                Database.Initialize();
            }

            _status.Target = DisplayWSSConnectionStatus;
            Client.SetWebSocketStatus(_status);

            //CreateOfficerInformationWindow(7272982);
        }

        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            // To prevent System.InvalidOperationException (already open associated DataReader)
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            // Choosing our particular provider
            mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            //Setting default settings
            mapView.MinZoom = 2;
            mapView.MaxZoom = 30;
            // Setting startposition to our neighborhood. 
            mapView.CenterPosition = new PointLatLng(52.506476, 6.097585);
            // Set initial zoom level
            mapView.Zoom = 14;
            // lets the map use the mousewheel to zoom
            mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            mapView.CanDragMap = true;
            // Deletes the red cross at the center of the map
            mapView.ShowCenter = false;
            // lets the user drag the map with the left mouse button
            mapView.DragButton = System.Windows.Input.MouseButton.Left;
            // keep zooming if cursor is over a marker
            mapView.IgnoreMarkerOnMouseWheel = true;

            // Declaring the Neigbourhood borders
            #region 'Assendorp' border polygon points
            List<PointLatLng> points = new(new PointLatLng[] {
                new PointLatLng(52.510338, 6.086134),
                new PointLatLng(52.510025, 6.089632),
                new PointLatLng(52.508336, 6.091026),
                new PointLatLng(52.509150, 6.094975),
                new PointLatLng(52.508742, 6.097353),
                new PointLatLng(52.512758, 6.101388),
                new PointLatLng(52.504651, 6.122414),
                new PointLatLng(52.500364, 6.126497),
                new PointLatLng(52.498655, 6.121036),
                new PointLatLng(52.498189, 6.107308),
                new PointLatLng(52.500130, 6.102842),
                new PointLatLng(52.500923, 6.098759),
                new PointLatLng(52.501388, 6.087123),
                new PointLatLng(52.501595, 6.084538),
                new PointLatLng(52.502654, 6.077440),
                new PointLatLng(52.503905, 6.073497)
            });
            #endregion

            // Creating polygon out of given & adding polygon to map
            mapView.Markers.Add(new GMapPolygonColor(points, Helper.FromArgb(200, 0, 0, 0), Helper.FromArgb(90, 255, 255, 255)));

            // start the main timer
            MainInstance.SetEventTimer();

            // set the control for the whole app to use
            MainInstance.SetControl(mapView);

            // set the bounds
            MainInstance.SetBounds(points);

            // fetch Offenses from db
            MainInstance.ReloadOffenses();
            // fetch cameras from db
            MainInstance.ReloadCameras();
            // fetch officers from db
            MainInstance.ReloadOfficers();

            this.DisplayNameTextBox.Text = (MainInstance.LoggedInEmployee != null) ? MainInstance.LoggedInEmployee.GetFullName() : "Not logged in";

            //testOfficer1.SetPatrolRoute(testPatrolRoute1);
            //testOfficer2.SetPatrolRoute(testPatrolRoute2);

            // Add delict heatmap
            Visualization.AddHeatmap();
            //Initial filter -> only Unprocessed offenses shown
            MapFilter MapFilter = new MapFilter(DateTime.UnixEpoch.Date, DateTime.Today.Date, TypeDropdown.SelectionBoxItem.ToString(), StatusDropdown.SelectionBoxItem.ToString());
            MapFilter.FilterOffenses();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs args)
        {
            MainInstance.Timer -= CheckLogin;
            SessionData.LogOut();
            LoginPage loginPage = new LoginPage();
            loginPage.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window != loginPage)
                {
                    window.Close();
                }
            }
        }

        public static void ViewLocation(PointLatLng coordinates)
        {
            MainInstance.GetControl().CenterPosition = new PointLatLng(0, 0); // is needed, else the propertyOnChanged does not get invoked.
            MainInstance.GetControl().CenterPosition = coordinates;
        }
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            if (Sidebar.Visibility == Visibility.Hidden)
            {
                Sidebar.Visibility = Visibility.Visible;
                MapGrid.Margin = new Thickness(299, 65, 0, 0);
            }
            else
            {
                Sidebar.Visibility = Visibility.Hidden;
                MapGrid.Margin = new Thickness(0, 65, 0, 0);
            }
        }

        public void AdminVisibility()
        {
            AddOffenseImg.RenderTransformOrigin = new Point(0.5, 0.5);
            if (SessionData.IsAdmin)
            {
                AdminButton.Visibility = Visibility.Visible;
                AddOffenseButton.Visibility = Visibility.Visible;
            }
            else
            {
                AdminButton.Visibility = Visibility.Hidden;
                AddOffenseButton.Visibility = Visibility.Hidden;
            }
        }


        private void AdminButton_OnClick(object sender, RoutedEventArgs e)
        {
            AdministratorWindow adminWindow = new();
            adminWindow.Show();
        }


        private void CheckLogin(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!SessionData.IsLoggedIn())
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {

                    LoginPage loginPage = new();
                    loginPage.Show();
                    MainInstance.Timer -= CheckLogin;
                    Close();
                });
            }
        }

        private Settings SettingsWin;

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsWin == null)
            {
                SettingsWin = new Settings();
                SettingsWin.Owner = this;
                SettingsWin.Closed += (a, b) => SettingsWin = null;
                SettingsWin.Closed += SettingsWin_Closed;
                SettingsWin.ShowDialog();
            }
            else
            {
                SettingsWin.ShowDialog();
            }
        }

        private void SettingsWin_Closed(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void AddOffense_Click(object sender, RoutedEventArgs e)
        {
            if (isMakingOffense)
            {
                isMakingOffense = false;
                mapView.Opacity = 1;
                AddOffenseText.Text = "Voeg delict toe";
                AddOffenseImg.RenderTransform = new RotateTransform(0);
            }
            else
            {
                isMakingOffense = true;
                mapView.Opacity = 0.5;
                AddOffenseText.Text = "Annuleren";
                AddOffenseImg.RenderTransform = new RotateTransform(45);

            }
        }

        private void MapView_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isMakingOffense)
            {
                PointLatLng point = mapView.FromLocalToLatLng(Convert.ToInt32(e.GetPosition(mapView).X),
                    Convert.ToInt32(e.GetPosition(mapView).Y));
                int id = OffenseDataContext.AddNewOffense(point);

                MainInstance.ReloadOffenses();

                Offense o = MainInstance.GetOffensesFromID(id);
                o.Draw();

                ContentUpdate contentUpdate = new(UpdateType.NewOffense)
                {
                    ContentUpdateNewOffense = new()
                    {
                        OffenseID = o.ID,
                        Status = o.Status,
                        OffenseType = o.Type,
                        Description = o.Description,
                        Point = point
                    },
                    AuthKey = SessionData.AuthKey
                };
                MainInstance.Client.SendMessage(contentUpdate);

                mapView.Opacity = 1;
                isMakingOffense = false;
                AddOffenseText.Text = "Voeg delict toe";
                AddOffenseImg.RenderTransform = new RotateTransform(0);
                Visualization.ReloadHeatmaps();
            }
        }
        private void OffenseButton_Click(object sender, RoutedEventArgs e)
        {
            OffenseDataContext.CreateOffenseHistoryWindow();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            //Checks if datepickers are filled in & Dates are valid.
            DateTime ToDate = (ToDatepicker.SelectedDate != null && ToDatepicker.SelectedDate < DateTime.Today.Date
                ? ToDatepicker.SelectedDate.Value
                : DateTime.Today.Date);

            DateTime FromDate = (FromDatepicker.SelectedDate != null && FromDatepicker.SelectedDate < ToDate.Date
                ? FromDatepicker.SelectedDate.Value
                : DateTime.UnixEpoch.Date);

            //Visually changes DatePicker value depending on given value
            ToDatepicker.SelectedDate = ToDate;

            MapFilter MapFilterArgs = new MapFilter(FromDate, ToDate, TypeDropdown.SelectionBoxItem.ToString(), StatusDropdown.SelectionBoxItem.ToString());

            //Initialize new Map Filter Arguments
            MapFilter MapFilter = new MapFilter(FromDate, ToDate, TypeDropdown.SelectionBoxItem.ToString(), StatusDropdown.SelectionBoxItem.ToString());
            MapFilter.FilterOffenses();

        }


        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FromDatepicker.SelectedDate = null;
            ToDatepicker.SelectedDate = null;
            TypeDropdown.SelectedIndex = 0;
            StatusDropdown.SelectedIndex = 1;

            MapFilter MapFilter = new MapFilter(DateTime.UnixEpoch.Date, DateTime.Today.Date, TypeDropdown.SelectionBoxItem.ToString(), StatusDropdown.SelectionBoxItem.ToString());

            MapFilter.FilterOffenses();

        }
    }
}